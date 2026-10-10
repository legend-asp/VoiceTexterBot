using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceTexterBot.Extensions
{
    public static class DirectoryExtension
    {
        /// <summary>
        /// Получаем путь до каталога с .sln файлом
        /// </summary>
        public static string GetSolutionRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                if (current.GetFiles("*.sln").Length > 0 || current.GetDirectories("Speech-models").Length > 0)
                {
                    return current.FullName;
                }
                current = current.Parent;
            }

            return AppContext.BaseDirectory;
        }
    }
}
