#region LGPL License

/*
Axiom Graphics Engine Library
Copyright � 2003-2011 Axiom Project Team

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

#endregion LGPL License

#region SVN Version Information

// <file>
//     <license see="http://axiom3d.net/wiki/index.php/license.txt"/>
//     <id value="$Id$"/>
// </file>

#endregion SVN Version Information

#region Namespace Declarations

using System.ComponentModel.Composition;
using Axiom.Core;
using Axiom.Graphics;

#endregion Namespace Declarations

namespace Axiom.RenderSystems.Xna
{
    /// <summary>
    /// Summary description for Plugin.
    /// </summary>
#if !( XBOX || XBOX360 || WINDOWS_PHONE )
    [ Export( typeof( IPlugin ) )]
#endif
    public sealed class Plugin : IPlugin
    {
        #region Fields

        /// <summary>
        ///     Reference to the render system instance.
        /// </summary>
        private RenderSystem _renderSystem;

        #endregion Fields

        #region Implementation of IPlugin

        public void Initialize()
        {
            // Render system creation has been moved here since the Plugin.ctor is called twice
            // during startup
            _renderSystem = new XnaRenderSystem();

            // add an instance of this plugin to the list of available RenderSystems
            Root.Instance.RenderSystems.Add( "Xna", _renderSystem );

            ResourceGroupManager.Instance.Initialize( new[]
                                                      {
                                                          "png", "jpg", "bmp", "dds", "jpeg", "tiff"
                                                      } );

            //new XnaMaterialManager();
        }

        public void Shutdown()
        {
            // nothing at the moment
            _renderSystem.SafeDispose();
            _renderSystem = null;
        }

        #endregion Implementation of IPlugin
    }
}