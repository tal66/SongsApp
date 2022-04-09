using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongsApp.Services
{
    public interface IFilesService
    {
        string[] SearchMp3FilesByPattern(string searchString, string rootDir);
        void OpenFile(string filename);
    }
}
