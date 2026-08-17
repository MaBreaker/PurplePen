using System;

namespace PurplePen.Livelox
{
    public static class ControlExtensions
    {
        public static void InvokeOnUiThread(this object control, Action action)
        {
            // In Avalonia, this is typically handled through the dispatcher
            // For now, just execute the action directly since Avalonia handles 
            // thread marshalling differently than Windows Forms
            action?.Invoke();
        }
    }
}