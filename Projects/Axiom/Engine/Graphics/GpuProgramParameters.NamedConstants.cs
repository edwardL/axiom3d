﻿#region LGPL License

/*
Axiom Graphics Engine Library
Copyright © 2003-2011 Axiom Project Team

The overall design, and a majority of the core engine and rendering code
contained within this library is a derivative of the open source Object Oriented
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

#endregion

#region SVN Version Information
// <file>
//     <license see="http://axiom3d.net/wiki/index.php/license.txt"/>
//     <id value="$Id:$"/>
// </file>
#endregion SVN Version Information

#region Namespace Declarations

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Axiom.Core;
using Axiom.Serialization;

#endregion Namespace Declarations

namespace Axiom.Graphics
{
	public partial class GpuProgramParameters
	{
		/// <summary>
		/// Simple class for loading / saving GpuNamedConstants
		/// </summary>
		public class GpuNamedConstantsSerializer : Serializer
		{
			public void ExportNamedConstants( GpuNamedConstants pConsts, string filename )
			{
#warning implement Endian.Native.
				ExportNamedConstants( pConsts, filename, Endian.Little );
			}

			public void ExportNamedConstants( GpuNamedConstants pConsts, string filename, Endian endianMode )
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="stream"></param>
			/// <param name="pDest"></param>
			public void ImportNamedConstants( Stream stream, GpuNamedConstants pDest )
			{
				throw new NotImplementedException();
			}
		}

        /// <summary>
        /// Find a constant definition for a named parameter.
        /// <remarks>
        /// This method returns null if the named parameter did not exist, unlike
        /// <see cref="GetConstantDefinition" /> which is more strict; unless you set the 
        /// last parameter to true.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name to look up</param>
        /// <param name="throwExceptionIfMissing"> If set to true, failure to find an entry
        /// will throw an exception.</param>
        public GpuConstantDefinition FindNamedConstantDefinition(string name, bool throwExceptionIfNotFound)
	    {

            if (namedParams == null)
		    {
                if (throwExceptionIfNotFound)
                    throw new AxiomException( "Named constants have not been initialised, perhaps a compile error." );
			    return null;
		    }

            int value;
            if (!namedParams.TryGetValue( name, out value ))
		    {
			    if (throwExceptionIfNotFound)
			        throw new AxiomException( "Parameter called " + name + " does not exist. " );
			    return null;
		    }
		    //else
	        {
                // temp hack (gotta update this mess)
	            var def = new GpuConstantDefinition();
	            def.LogicalIndex = value;
	            def.PhysicalIndex = value;
	            return def;
	            //return &(i->second);
	        }
	    }
	}
}