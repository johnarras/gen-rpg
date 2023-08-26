using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class WeatherType : IIndexedGameItem
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }

        [Key(5)] public MyColorF AmbientColor { get; set; }
        [Key(6)] public MyColorF LightColor { get; set; }
        [Key(7)] public float LightScale { get; set; }
        [Key(8)] public MyColorF SkyColor { get; set; }
        [Key(9)] public float FogScale { get; set; }
        [Key(10)] public MyColorF FogColor { get; set; }
        [Key(11)] public float FogDistance { get; set; }
        [Key(12)] public float CloudScale { get; set; }
        [Key(13)] public float CloudSpeed { get; set; }
        [Key(14)] public MyColorF CloudColor { get; set; }

        [Key(15)] public float WindScale { get; set; }

        [Key(16)] public float PrecipScale { get; set; }
        [Key(17)] public bool IsCold { get; set; }

        [Key(18)] public float ParticleScale { get; set; }
        [Key(19)] public string Particles { get; set; }

        public WeatherType()
        {
            AmbientColor = new MyColorF();
            LightColor = new MyColorF();
            LightScale = 1.0f;
            SkyColor = new MyColorF();
            FogScale = 0f;
            FogColor = new MyColorF();
            CloudScale = 0f;
            CloudSpeed = 0f;
            WindScale = 1.0f;
            PrecipScale = 1.0f;
            Particles = "";
        }

    }
}
