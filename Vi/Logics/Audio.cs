using System;
using System.IO;
using WMPLib;

namespace Vi.Logics
{
    public sealed class Audio
    {
        /// <summary>
        /// Название аудио
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Длительность аудио в секундах
        /// </summary>
        public double Duration { get; private set; }
        /// <summary>
        /// Длительность аудио в формате TimeSpan
        /// </summary>
        public TimeSpan DurationTime => TimeSpan.FromSeconds(Duration);
        /// <summary>
        /// Путь к аудио файлу
        /// </summary>
        public string SourceUrl { get; private set; }
        /// <summary>
        /// Объект IWMPMedia
        /// </summary>
        public IWMPMedia Media { get; private set; }

        public Audio(IWMPMedia media)
        {
            Media = media;
            Name = Path.GetFileNameWithoutExtension(Media.sourceURL);
            Duration = Media.duration;
            SourceUrl = Media.sourceURL;
        }

    }
}