using Microsoft.UI.Xaml.Controls;
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
