using Microsoft.Identity.Client;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Genrpg.Editor.UI
{
    public class MessageBoxWaiter
    {

        public IAsyncOperation<ContentDialogResult> Operation { get; set; } = null;
        public bool DidSetOperation { get; set; } = false;
        public ContentDialogResult Result { get; set; } = ContentDialogResult.None;
    }
}
