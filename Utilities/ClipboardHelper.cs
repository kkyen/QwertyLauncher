using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace QwertyLauncher.Utilities
{
    internal class ClipboardHelper
    {
        internal static void SetClipboardText(string text)
        {
            Thread t = new Thread(() => {
                int retryCount = 5;
                while (retryCount-- > 0)
                {
                    try
                    {
                        Clipboard.SetText(text);
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(100); // 100ms待機してリトライ
                    }
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        internal static string GetClipboardText()
        {
            var text = "";
            Thread t = new Thread(() => {
                int retryCount = 5;
                while (retryCount-- > 0)
                {
                    Debug.Print("copy");
                    try
                    {
                        text = Clipboard.GetText();
                        if(text.Length != 0) break;
                    }
                    catch
                    {
                        Thread.Sleep(100); // 500ms待機してリトライ
                    }
                    Thread.Sleep(100);
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            return text;
        }

        internal static void ClearClipboard()
        {
            Thread t = new Thread(() =>
            {
                int retryCount = 5;
                while (retryCount-- > 0)
                {
                    try
                    {
                        Clipboard.Clear();
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(100); // 100ms待機してリトライ
                    }
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }
}
