using System;
using System.Collections.Generic;
using System.Text;

namespace Msfs.ControllerVisualizer.Common
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Class NotifyPropertyBase. Implements the <see cref="System.ComponentModel.INotifyPropertyChanged"/>
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
    public class NotifyPropertyBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Called when [notify property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        internal void OnNotifyPropertyChanged([CallerMemberName] string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
