using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Msr.Mlas.SpecialFunctions 
{ 
    public class TemporaryDirectory : IDisposable
    { 
        private TemporaryDirectory()
        {
        }

        public string Name { private set; get; }
        public bool CleanUp { private set; get; } 
        public static TemporaryDirectory GetInstance() 
        {
            return GetInstance(Path.GetTempPath(), true); 
        }

        public static TemporaryDirectory GetInstance(string parentOfTempDirectory, bool cleanUp)
        {
            TemporaryDirectory temporaryDirectory = new TemporaryDirectory();
            temporaryDirectory.Name = Path.Combine(parentOfTempDirectory, Path.GetRandomFileName()); 
            temporaryDirectory.CleanUp = cleanUp; 
            Directory.CreateDirectory(temporaryDirectory.Name);
            return temporaryDirectory; 
        }

        #region IDisposable Members

        public void Dispose()
        { 
            if (CleanUp) 
            {
                Directory.Delete(Name, true); 
            }
        }

        #endregion

        override public string ToString() 
        { 
            return Name;
        } 
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
