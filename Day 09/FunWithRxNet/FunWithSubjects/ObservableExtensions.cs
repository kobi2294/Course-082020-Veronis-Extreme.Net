﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunWithSubjects
{
    public static class ObservableExtensions
    {
        private static int _counter = 0;

        public static void Log(this string str, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = prev;
        }

        public static void LogAt(this string str, ConsoleColor color, int row, int column)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.CursorLeft = column;
            Console.CursorTop = row;
            Console.Write(str);
            Console.ForegroundColor = prev;
        }

        private static void LogAt<T>(ConsoleColor color, int row, (Notification<T> notification, long time) pair)
        {
            string txt = "";
            switch (pair.notification.Kind)
            {
                case NotificationKind.OnNext:
                    txt = pair.notification.Value.ToString();
                    break;
                case NotificationKind.OnError:
                    txt = "X";
                    break;
                case NotificationKind.OnCompleted:
                    txt = "|";
                    break;
                default:
                    break;
            }

            var col = (int)pair.time * 5 + 10;

            var windowWidth = Console.WindowWidth - 5;
            var actualRow = row + (col / windowWidth) * (_counter + 1);
            var actualCol = col % windowWidth;

            txt.LogAt(color, actualRow, actualCol);
        }


        public static IDisposable SubscribeConsole<T>(this IObservable<T> source, string prefix = "", ConsoleColor color = ConsoleColor.White)
        {
            return source
                .Subscribe(
                val => $"{prefix} Next: {val}".Log(color),
                err => $"{prefix} Error: {err.Message}".Log(color),
                () => $"{prefix} Completed".Log(color));                
        }

        public static IDisposable SubscribeMarble<T>(this IObservable<T> source, 
            string prefix = "", 
            ConsoleColor color = ConsoleColor.White)
        {
            var row = Interlocked.Increment(ref _counter);
            prefix.LogAt(color, row, 0);

            return source
                .Materialize()
                .WithLatestFrom(Observable.Interval(TimeSpan.FromSeconds(1)), (notification, time) => (notification, time))
                .Subscribe(pair => LogAt(color, row, pair));
        }
    }
}
