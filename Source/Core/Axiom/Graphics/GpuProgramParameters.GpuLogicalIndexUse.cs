﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axiom.Graphics
{
	public partial class GpuProgramParameters
	{
		[OgreVersion( 1, 7, 2790 )]
		public class GpuLogicalIndexUse
		{
			#region PhysicalIndex

			/// <summary>
			/// Physical buffer index
			/// </summary>
			[OgreVersion( 1, 7, 2790 )] public int PhysicalIndex;

			#endregion

			#region CurrentSize

			/// <summary>
			/// Current physical size allocation
			/// </summary>
			[OgreVersion( 1, 7, 2790 )] public int CurrentSize;

			#endregion

			#region Variability

			/// <summary>
			/// How the contents of this slot vary
			/// </summary>
			[OgreVersion( 1, 7, 2790 )] public GpuParamVariability Variability;

			#endregion

			#region Constructor

			[OgreVersion( 1, 7, 2790 )]
			public GpuLogicalIndexUse()
			{
				this.PhysicalIndex = 99999;
				this.CurrentSize = 0;
				this.Variability = GpuParamVariability.Global;
			}

			[OgreVersion( 1, 7, 2790 )]
			public GpuLogicalIndexUse( int bufIdx, int curSz, GpuParamVariability v )
			{
				this.PhysicalIndex = bufIdx;
				this.CurrentSize = curSz;
				this.Variability = v;
			}

			#endregion

			[AxiomHelper( 0, 8 )]
			public GpuLogicalIndexUse Clone()
			{
				var p = new GpuLogicalIndexUse();
				p.PhysicalIndex = this.PhysicalIndex;
				p.CurrentSize = this.CurrentSize;
				p.Variability = this.Variability;
				return p;
			}
		}

		[OgreVersion( 1, 7, 2790 )]
		public class GpuLogicalIndexUseMap : Dictionary<int, GpuLogicalIndexUse>
		{
		}
	}
}