using System; 
using System.Collections.Generic;
using System.Text;

namespace VirusCount.PhyloTree
{
    public abstract class EditRow 
    { 
        public abstract void EditHeader(ref string header);
        public abstract void Edit(Dictionary<string, string> row); 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
