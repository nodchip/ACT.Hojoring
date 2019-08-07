using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Globalization;
using Prism.Mvvm;
using Google.Cloud.TextToSpeech.V1;


namespace ACT.TTSYukkuri.Config
{
    [Serializable]
    public class GoogleCloudTextToSpeechConfig :
        BindableBase
    {
        private TextToSpeechClient client = TextToSpeechClient.Create();

        private string languageCode;
        private string name;
        private double volumeGainDb;
        private double pitch;
        private double speakingRate;
        private int sampleRateHertz;

        public GoogleCloudTextToSpeechConfig()
        {
            this.SetRecommend();
        }

        /// <summary>
        /// 言語 [BCP-47]
        /// </summary>
        public string LanguageCode
        {
            get => this.languageCode;
            set
            {
                this.SetProperty(ref this.languageCode, value);
                // Raise to update the contents of the language code combo box.
                RaisePropertyChanged("LanguageCodeList");
            }
        }

        /// <summary>
        /// 音声名
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        /// <summary>
        /// ボリューム [db] -96.0 ～ 16.0
        /// </summary>
        public double VolumeGainDb
        {
            get => this.volumeGainDb;
            set => this.SetProperty(ref this.volumeGainDb, value);
        }

        /// <summary>
        /// ピッチ -20.0 ～ 20.0
        /// </summary>
        public double Pitch
        {
            get => this.pitch;
            set => this.SetProperty(ref this.pitch, value);
        }

        /// <summary>
        /// レート 0.25 ～ 4.0
        /// </summary>
        public double SpeakingRate
        {
            get => this.speakingRate;
            set => this.SetProperty(ref this.speakingRate, value);
        }

        /// <summary>
        /// サンプリング周波数
        /// </summary>
        public int SampleRateHertz
        {
            get => this.sampleRateHertz;
            set => this.SetProperty(ref this.sampleRateHertz, value);
        }

        /// <summary>
        /// 推奨値を設定する
        /// </summary>
        public void SetRecommend()
        {
            this.languageCode = "ja-JP";
            this.name = "ja-JP-Wavenet-A";
            this.volumeGainDb = 0.0;
            this.pitch = 0.0;
            this.speakingRate = 1.0;
            this.sampleRateHertz = 44100;
        }

        public GoogleCloudTextToSpeechLanguageCode[] EnumerateLanguageCode()
        {
            // C# で国名の一覧を取得・表示する - ディーバ Blog https://blog.divakk.co.jp/entry/2017/01/23/123226
            return CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Where(c => c.LCID != 4096 /* LOCALE_CUSTOM_UNSPECIFIED を除く */)
                .OrderBy(c => c.DisplayName)
                .Select(c => new GoogleCloudTextToSpeechLanguageCode { Info = c })
                .ToArray();
        }

        public GoogleCloudTextToSpeechVoice[] EnumerateVoice()
        {
            return client
                .ListVoices(languageCode)
                .Voices
                .Select(x => new GoogleCloudTextToSpeechVoice { VoiceName = x.Name })
                .ToArray();
        }

        public GoogleCloudTextToSpeechSampleRateHertz[] EnumerateSampleRateHertz()
        {
            return new int[] { 8000, 11025, 16000, 22050, 32000, 44100, 48000, 96000, 192000 }
                .Select(x => new GoogleCloudTextToSpeechSampleRateHertz { SampleRateHertz = x })
                .ToArray();
        }

        public override string ToString() =>
            $"{nameof(this.LanguageCode)}:{this.LanguageCode}," +
            $"{nameof(this.Name)}:{this.Name}," +
            $"{nameof(this.VolumeGainDb)}:{this.VolumeGainDb}," +
            $"{nameof(this.Pitch)}:{this.Pitch}," +
            $"{nameof(this.SpeakingRate)}:{this.SpeakingRate}," +
            $"{nameof(this.SampleRateHertz)}:{this.SampleRateHertz}";
    }

    [Serializable]
    public class GoogleCloudTextToSpeechLanguageCode
    {
        public string Value => Info.DisplayName;

        public string Name => Info.Name;

        public CultureInfo Info { get; set; }

        public override string ToString() => this.Name;
    }

    [Serializable]
    public class GoogleCloudTextToSpeechVoice
    {
        public string Value => VoiceName;

        public string Name => VoiceName;

        public string VoiceName { get; set; }

        public override string ToString() => this.Name;
    }

    [Serializable]
    public class GoogleCloudTextToSpeechSampleRateHertz
    {
        public int Value => SampleRateHertz;

        public string Name => SampleRateHertz.ToString();

        public int SampleRateHertz { get; set; }

        public override string ToString() => this.Name;
    }
}
