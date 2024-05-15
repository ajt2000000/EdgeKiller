using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;

class Program
{
    static void Main()
    {
        // 管理者権限をチェック
        if (!IsAdministrator())
        {
            // 管理者権限を要求する
            ElevateToAdministrator();
            return;
        }

        // Microsoft Edgeのインストールをチェック
        if (!IsEdgeInstalled())
        {
            Console.WriteLine("Microsoft Edgeがインストールされていません。プログラムを終了します。");
            return;
        }

        Console.WriteLine("Microsoft Edgeを削除しますか？ (y/n) (5秒以内に応答してください)");

        // タイムアウト付きでユーザー入力を取得
        string input = ReadLineWithTimeout(5000);

        if (input?.ToLower() == "y")
        {
            // Microsoft Edgeのプロセスを停止
            StopEdgeProcesses();

            // ファイルとディレクトリの削除
            string[] edgePaths = {
                @"C:\Program Files (x86)\Microsoft\Edge\",
                @"C:\Program Files\Microsoft\Edge\"
            };

            foreach (var path in edgePaths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                        Console.WriteLine($"ディレクトリ {path} を削除しました。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラー: {path} の削除に失敗しました。{ex.Message}");
                }
            }

            // レジストリキーの削除
            string[] registryKeys = {
                @"SOFTWARE\Microsoft\EdgeUpdate",
                @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate"
            };

            foreach (var key in registryKeys)
            {
                try
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key, true);
                    if (registryKey != null)
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(key);
                        Console.WriteLine($"レジストリキー {key} を削除しました。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラー: {key} の削除に失敗しました。{ex.Message}");
                }
            }

            // ユーザーデータの削除
            string userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data");
            try
            {
                if (Directory.Exists(userDataPath))
                {
                    Directory.Delete(userDataPath, true);
                    Console.WriteLine($"ユーザーデータディレクトリ {userDataPath} を削除しました。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラー: ユーザーデータディレクトリ {userDataPath} の削除に失敗しました。{ex.Message}");
            }

            Console.WriteLine("Microsoft Edgeの削除が完了しました。");
        }
        else
        {
            Console.WriteLine("削除がキャンセルされました。");
        }
    }

    private static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void ElevateToAdministrator()
    {
        var exeName = Process.GetCurrentProcess().MainModule.FileName;
        var startInfo = new ProcessStartInfo(exeName)
        {
            Verb = "runas",
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("管理者権限を要求する際にエラーが発生しました。: " + ex.Message);
        }
    }

    private static bool IsEdgeInstalled()
    {
        string[] edgePaths = {
            @"C:\Program Files (x86)\Microsoft\Edge\",
            @"C:\Program Files\Microsoft\Edge\"
        };

        foreach (var path in edgePaths)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
        }
        return false;
    }

    private static string ReadLineWithTimeout(int timeout)
    {
        string input = null;
        Thread inputThread = new Thread(() => input = Console.ReadLine());
        inputThread.Start();

        bool completed = inputThread.Join(timeout);
        if (!completed)
        {
            inputThread.Interrupt();
            Console.WriteLine("\nタイムアウトしました。");
        }

        return input;
    }

    private static void StopEdgeProcesses()
    {
        try
        {
            foreach (var process in Process.GetProcessesByName("msedge"))
            {
                process.Kill();
                process.WaitForExit();
                Console.WriteLine("Microsoft Edgeのプロセスを停止しました。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Microsoft Edgeのプロセスを停止する際にエラーが発生しました: {ex.Message}");
        }
    }
}
