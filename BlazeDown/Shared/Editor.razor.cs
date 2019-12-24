using Microsoft.AspNetCore.Components;
using System.Timers;

namespace BlazeDown.Shared
{
    public class EditorBase : ComponentBase
    {
        Timer debounceTimer = new Timer()
        {
            Interval = 1000,
            AutoReset = false,
        };

        [Parameter] public string InitialValue { get; set; }

        private object finalValue;

        [Parameter] public EventCallback<ChangeEventArgs> OnChange { get; set; }

        protected void InputChanged(ChangeEventArgs args) 
        {
            if (!debounceTimer.Enabled)
            {
                StartDebounceTimer();
            }
            else
            {
                ExtendDebounceTimer(args);
            }
        }

        private void ExtendDebounceTimer(ChangeEventArgs args)
        {
            finalValue = args.Value;
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private void StartDebounceTimer()
        {
            debounceTimer.Elapsed += (_, a) =>
                OnChange.InvokeAsync(new ChangeEventArgs { Value = finalValue });

            debounceTimer.Start();
        }
    }
}
