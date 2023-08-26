using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// Enums for different categories of music/ambient sounds depending on how much you want to
/// layer them,
/// 
/// The info about the looping/randomization is in the main prefab MusicChannels list.
/// 
/// If you add an element here, be sure to update 
/// 
/// 1. IMusicRegion interface with a new *MusicTypeId
/// 2. The PlayMusic function where the little dictionary of Ids is set up. Search for
///     the Music enum below to see where it's used to map from IMusicRegion to the categories.
/// </summary>
public enum AudioCategory
{
    Sound,
    Music,
    Ambient,
}
