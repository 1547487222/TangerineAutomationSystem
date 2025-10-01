using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.TangerineProject
{
    public interface ITangerineProgram
    {
        void InitProgram();

        void StoreProgram();

        void InitializeCinemaDesignDslSource();

        void UnInitializeCinemaDesignDslSource();

        //TangerineTheatreSystem TheatreSystem { get; }

        void CreateProgram(string rootPath, string solutionName, string projectName, string desc);

        void CrammTemplateInfo();
    }
}
