using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using VirusCount.PhyloTree;

namespace PhyloDDN
{
    /// <summary>
    /// Represents the input data for a run of <see cref="PhyloTree"/>.
    /// </summary>
    /// <remarks>Note that this class intentionally avoids using any non-native .NET types.  This was an act of convenience so that 
    /// related types (e.g., <see cref="RangeCollection"/>) would not need to be made serializable.
    /// </remarks>
    [Serializable]
    public class InputData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputData"/> class.
        /// </summary>
        /// <param name="optimizerName">Name of the optimizer.</param>
        /// <param name="keepTestName">Name of the keep test.</param>
        /// <param name="leafDistributionName">Name of the leaf distribution.</param>
        /// <param name="nullDataGeneratorName">Name of the null data generator.</param>
        /// <param name="niceName">Name of the nice.</param>
        /// <param name="nullIndexRange">The null index range.</param>
        /// <param name="localOutputDirectoryName"></param>
        /// <param name="pieceIndex">Index of the piece.</param>
        /// <param name="pieceCount">The piece count.</param>
        /// <remarks>This constructor is called by the master application when creating <see cref="PhyloDWorker"/> objects.</remarks>
        public InputData(string optimizerName, string keepTestName, string leafDistributionName, string nullDataGeneratorName, string niceName, string localOutputDirectoryName, int pieceIndex, int pieceCount, string nullIndexRange)
        {
            mOptimizerName = optimizerName;
            mKeepTestName = keepTestName;
            mLeafDistributionName = leafDistributionName;
            mNullDataGeneratorName = nullDataGeneratorName;
            mNiceName = niceName;
            mPieceIndex = pieceIndex;
            mPieceIndexRange = string.Format(CultureInfo.InvariantCulture, "{0}-{0}", pieceIndex);
            mPieceCount = pieceCount;
            mNullIndexRange = nullIndexRange;
            mLocalOutputDirectoryName = localOutputDirectoryName;
        }

        // Serialized input fields
        private string mOptimizerName;
        private string mKeepTestName;
        private string mLeafDistributionName;
        private string mNullDataGeneratorName;
        private string mNiceName;
        private int mPieceIndex;
        private string mPieceIndexRange;
        private int mPieceCount;
        private string mNullIndexRange;
        private string mLocalOutputDirectoryName;

        // properties
        public string OptimizerName
        {
            get { return mOptimizerName; }
        }

        public string KeepTestName
        {
            get { return mKeepTestName; }
        }

        public string NullIndexRange
        {
            get { return mNullIndexRange; }
        }

        public string NiceName
        {
            get { return mNiceName; }
        }

        public string LeafDistributionName
        {
            get { return mLeafDistributionName; }
        }

        public string NullDataGeneratorName
        {
            get { return mNullDataGeneratorName; }
        }

        public string PieceIndexRange
        {
            get { return mPieceIndexRange; }
        }

        public int PieceIndex
        {
            get { return mPieceIndex; }
        }

        public int PieceCount
        {
            get { return mPieceCount; }
        }

        public string LocalOutputDirectoryName
        {
            get { return mLocalOutputDirectoryName; }
        }

    }
}
