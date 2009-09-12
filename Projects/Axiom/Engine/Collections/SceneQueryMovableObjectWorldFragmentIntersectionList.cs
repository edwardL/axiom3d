#region LGPL License

// Axiom Graphics Engine Library
// Copyright (C) 2003-2009 Axiom Project Team
// 
// The overall design, and a majority of the core engine and rendering code 
// contained within this library is a derivative of the open source Object Oriented 
// Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
// Many thanks to the OGRE team for maintaining such a high quality project.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

#endregion

#region SVN Version Information

// <file>
//     <license see="http://axiomengine.sf.net/wiki/index.php/license.txt"/>
//     <id value="$Id$"/>
// </file>

#endregion SVN Version Information

#region Namespace Declarations

using Axiom.Core;

#endregion

namespace Axiom.Collections
{
    /// <summary>
    /// Represents a pair of a <see cref="MovableObject"/> and a <see cref="SceneQuery.WorldFragment"/>.
    /// </summary>
    public class SceneQueryMovableObjectWorldFragmentPair
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneQueryMovableObjectWorldFragmentPair"/> class.
        /// </summary>
        /// <param name="obj">A <see cref="MovableObject"/>.</param>
        /// <param name="fragment">A <see cref="SceneQuery.WorldFragment"/>.</param>
        public SceneQueryMovableObjectWorldFragmentPair( MovableObject obj, SceneQuery.WorldFragment fragment )
        {
            this.obj = obj;
            this.fragment = fragment;
        }

        #endregion

        #region Instance Properties

        /// <summary>
        /// Gets or sets the <see cref="SceneQuery.WorldFragment"/>.
        /// </summary>
        /// <value>A <see cref="SceneQuery.WorldFragment"/>.</value>
        public SceneQuery.WorldFragment fragment { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MovableObject"/>.
        /// </summary>
        /// <value>A <see cref="MovableObject"/>.</value>
        public MovableObject obj { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents a collection of <see cref="SceneQueryMovableObjectWorldFragmentPair">SceneQueryMovableObjectWorldFragmentPairs</see> that are sorted by name.
    /// </summary>
    public class SceneQueryMovableObjectWorldFragmentIntersectionList : AxiomCollection<SceneQueryMovableObjectWorldFragmentPair>
    {
    }
}