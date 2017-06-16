using System;
using Newtonsoft.Json;

namespace RetoFinal
{
    public class SCT_Tic_Mto
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Asunto")]
        public string Asunto { get; set; }

        [JsonProperty(PropertyName = "Liberado")]
        public bool Liberado { get; set; }

        [JsonProperty(PropertyName = "FechaLiberado")]
        public DateTime FechaLiberado { get; set; }

        [JsonProperty(PropertyName = "UsuarioId")]
        public int UsuarioId { get; set; }

        [JsonProperty(PropertyName = "Quien")]
        public string Quien { get; set; }

        [JsonProperty(PropertyName = "QuienLibera")]
        public string QuienLibera { get; set; }
    }

    public class SCT_Tic_MtoWrapper : Java.Lang.Object
    {
        public SCT_Tic_MtoWrapper(SCT_Tic_Mto item)
        {
            SCT_Tic_Mto = item;
        }

        public SCT_Tic_Mto SCT_Tic_Mto { get; private set; }
    }
}

