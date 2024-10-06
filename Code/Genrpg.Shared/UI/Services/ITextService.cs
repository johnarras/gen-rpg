using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;

namespace Genrpg.Shared.UI.Services
{
    public interface ITextService : IInjectable
    {
        string HighlightText(string text, string color = TextColors.ColorYellow);
        string HighlightText(char c, string color = TextColors.ColorYellow);
    }
}
