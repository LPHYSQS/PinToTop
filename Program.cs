using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PinToTop;

internal abstract partial class Program
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    const uint SwpNomove = 0x0002;
    const uint SwpNosize = 0x0001;
    const int HwndTopmost = -1;
    const int HwndNotopmost = -2;

    private static readonly StringBuilder StringBuilder = new();

    private static void Main()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("请选择要处理的进程：");
            Console.ResetColor();
            ListInteractiveProcesses();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("请输入进程编号（输入 'exit' 退出程序）：");
            Console.ResetColor();
            string? processIndexInput = Console.ReadLine();

            if (processIndexInput?.ToLower() == "exit")
                break;

            if (processIndexInput != null && !MyRegex().IsMatch(processIndexInput))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("请输入有效的数字。");
                Console.ResetColor();
                continue;
            }

            if (processIndexInput != null)
            {
                var processIndex = int.Parse(processIndexInput);
                if (_interactiveProcesses != null && processIndex >= 0 && processIndex < _interactiveProcesses.Length)
                {
                    var process = _interactiveProcesses[processIndex];
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"已选择进程：{process.ProcessName}");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("请选择要执行的操作（1. 置顶窗口 2. 取消置顶窗口）：");
                    Console.ResetColor();
                    var actionInput = Console.ReadLine();

                    if (actionInput is "1" or "2")
                        ProcessAction(process.MainWindowHandle, actionInput == "1");
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("无效的操作。");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("无效的进程编号。");
                    Console.ResetColor();
                }
            }
        }
    }

    private static Process[]? _interactiveProcesses;

    private static void ListInteractiveProcesses()
    {
        StringBuilder.Clear();
        _interactiveProcesses = GetInteractiveProcesses();
        for (var i = 0; i < _interactiveProcesses.Length; i++)
        {
            StringBuilder.AppendLine($"[{i}] {_interactiveProcesses[i].ProcessName}");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(StringBuilder.ToString());
        Console.ResetColor();
    }

    private static Process[] GetInteractiveProcesses()
    {
        return Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero).ToArray();
    }

    private static void ProcessAction(IntPtr hWnd, bool setTopMost)
    {
        try
        {
            var hWndInsertAfter = setTopMost ? HwndTopmost : HwndNotopmost;
            const uint flags = SwpNomove | SwpNosize;

            if (!SetWindowPos(hWnd, new IntPtr(hWndInsertAfter), 0, 0, 0, 0, flags))
                throw new InvalidOperationException("无法设置窗口置顶。");

            var action = setTopMost ? "置顶" : "取消置顶";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"窗口已{action}。");
            Console.ResetColor();
        }
        catch (Win32Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"无法设置窗口置顶：{ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"处理过程中发生错误：{ex.Message}");
            Console.ResetColor();
        }
    }

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex MyRegex();
}