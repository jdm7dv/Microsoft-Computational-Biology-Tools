using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Digipede.Framework;

namespace PhyloDDN
{
    public static class Utility
    {
        /// <summary>
        /// Gets a <see cref="FileDef"/> from <paramref name="fileDefs"/> that matches <paramref name="name"/>.
        /// </summary>
        /// <param name="fileDefs">The file defs.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static FileDef GetNamedFileDef(FileDefCollection fileDefs, string name) {
            if (fileDefs == null) {
                throw new ArgumentNullException("fileDefs");
            }
            int index = fileDefs.IndexOf(name);
            if (index == -1) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "No FileDef in the given collection has name '{0}'.", name),
                                            "name");
            }
            return fileDefs[index];
            
        }

    }
}
