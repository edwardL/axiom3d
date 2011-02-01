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
#endregion

#region SVN Version Information
// <file>
//     <license see="http://axiom3d.net/wiki/index.php/license.txt"/>
//     <id value="$Id$"/>
// </file>
#endregion SVN Version Information

#region Namespace Declarations

using System;
using Axiom.Collections;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Math;
using Axiom.Scripting.Compiler.AST;

#endregion Namespace Declarations

namespace Axiom.Scripting.Compiler
{
    public partial class ScriptCompiler
    {
        public class GpuProgramTranslator : Translator
        {
            public GpuProgramTranslator()
                : base()
            {
            }

            private GpuProgramType _translateIDToGpuProgramType( uint id )
            {
                switch ( (Keywords)id )
                {
                    case Keywords.ID_VERTEX_PROGRAM:
                    default:
                        return GpuProgramType.Vertex;

                    case Keywords.ID_GEOMETRY_PROGRAM:
                        return GpuProgramType.Geometry;

                    case Keywords.ID_FRAGMENT_PROGRAM:
                        return GpuProgramType.Fragment;
                }
            }

            #region Translator Implementation

            /// <see cref="Translator.CheckFor"/>
            internal override bool CheckFor( Keywords nodeId, Keywords parentId )
            {
                return nodeId == Keywords.ID_FRAGMENT_PROGRAM || nodeId == Keywords.ID_VERTEX_PROGRAM || nodeId == Keywords.ID_GEOMETRY_PROGRAM;
            }

            /// <see cref="Translator.Translate"/>
            public override void Translate( ScriptCompiler compiler, AbstractNode node )
            {
                ObjectAbstractNode obj = (ObjectAbstractNode)node;
                if ( obj != null )
                {
                    if ( string.IsNullOrEmpty( obj.Name ) )
                    {
                        compiler.AddError( CompileErrorCode.ObjectNameExpected, obj.File, obj.Line,
                            "gpu program object must have names" );

                        return;
                    }
                }
                else
                {
                    compiler.AddError( CompileErrorCode.ObjectNameExpected, obj.File, obj.Line,
                        "gpu program object must have names" );
                    
                    return;
                }

                // Must have a language type
                if ( obj.Values.Count == 0 )
                {
                    compiler.AddError( CompileErrorCode.StringExpected, obj.File, obj.Line,
                        "gpu program object require language declarations" );
                    return;
                }

                // Get the language
                string language;
                if ( !getString( obj.Values[ 0 ], out language ) )
                {
                    compiler.AddError( CompileErrorCode.InvalidParameters, obj.File, obj.Line );
                    return;
                }

                if ( language == "asm" )
                    _translateGpuProgram( compiler, obj );

                else if ( language == "unified" )
                    _translateUnifiedGpuProgram( compiler, obj );

                else
                    _translateHighLevelGpuProgram( compiler, obj );
            }

            #endregion Translator Implementation

            public static void TranslateProgramParameters( ScriptCompiler compiler, /*it should be GpuProgramParametersShared*/ GpuProgramParameters parameters, ObjectAbstractNode obj )
            {
                throw new NotImplementedException();

                int animParametricsCount = 0;

                foreach ( AbstractNode i in obj.Children )
                {
                    if ( i.Type == AbstractNodeType.Property )
                    {
                        PropertyAbstractNode prop = (PropertyAbstractNode)i;
                        switch ( (Keywords)prop.Id )
                        {
                            #region ID_SHARED_PARAMS_REF
                            case Keywords.ID_SHARED_PARAMS_REF:
                                {
                                    if ( prop.Values.Count != 1 )
                                    {
                                        compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                            "shared_params_ref requires a single parameter" );
                                        continue;
                                    }

                                    AbstractNode i0 = getNodeAt( prop.Values, 0 );
                                    if ( i0.Type != AbstractNodeType.Atom )
                                    {
                                        compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                            "shared parameter set name expected" );
                                        continue;
                                    }
                                    AtomAbstractNode atom0 = (AtomAbstractNode)i0;

                                    try
                                    {
                                        //parameters->addSharedParameters(atom0->value);
                                    }
                                    catch ( AxiomException e )
                                    {
                                        compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line, e.Message );
                                    }
                                }
                                break;
                            #endregion ID_SHARED_PARAMS_REF

                            #region ID_PARAM_INDEXED || ID_PARAM_NAMED
                            case Keywords.ID_PARAM_INDEXED:
                            case Keywords.ID_PARAM_NAMED:
                                {
                                    if ( prop.Values.Count >= 3 )
                                    {
                                        bool named = ( prop.Id == (uint)Keywords.ID_PARAM_NAMED );
                                        AbstractNode i0 = getNodeAt( prop.Values, 0 ), i1 = getNodeAt( prop.Values, 1 ),
                                            k = getNodeAt( prop.Values, 2 );

                                        if ( i0.Type != AbstractNodeType.Atom || i1.Type != AbstractNodeType.Atom )
                                        {
                                            compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                "name or index and parameter type expected" );
                                            return;
                                        }

                                        AtomAbstractNode atom0 = (AtomAbstractNode)i0, atom1 = (AtomAbstractNode)i1;
                                        if ( !named && !atom0.IsNumber )
                                        {
                                            compiler.AddError( CompileErrorCode.NumberExpected, prop.File, prop.Line,
                                                "parameter index expected" );
                                            return;
                                        }

                                        string name = string.Empty;
                                        int index = 0;
                                        // Assign the name/index
                                        if ( named )
                                            name = atom0.Value;
                                        else
                                            index = (int)atom0.Number;

                                        // Determine the type
                                        if ( atom1.Value == "matrix4x4" )
                                        {
                                            Matrix4 m;
                                            if ( getMatrix4( prop.Values, 2, out m ) )
                                            {
                                                try
                                                {
                                                    if ( named )
                                                        parameters.SetNamedConstant( name, m );
                                                    else
                                                        parameters.SetConstant( index, m );
                                                }
                                                catch
                                                {
                                                    compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                        "setting matrix4x4 parameter failed" );
                                                }
                                            }
                                            else
                                            {
                                                compiler.AddError( CompileErrorCode.NumberExpected, prop.File, prop.Line,
                                                    "incorrect matrix4x4 declaration" );
                                            }
                                        }
                                        else
                                        {
                                            // Find the number of parameters
                                            bool isValid = true;
                                            GpuProgramParameters.ElementType type = GpuProgramParameters.ElementType.Real;
                                            int count = 0;
                                            if ( atom1.Value.Contains( "float" ) )
                                            {
                                                type = GpuProgramParameters.ElementType.Real;
                                                if ( atom1.Value.Length >= 6 )
                                                    count = int.Parse( atom1.Value.Substring( 5 ) );
                                                else
                                                {
                                                    count = 1;
                                                }
                                            }
                                            else if ( atom1.Value.Contains( "int" ) )
                                            {
                                                type = GpuProgramParameters.ElementType.Int;
                                                if ( atom1.Value.Length >= 4 )
                                                    count = int.Parse( atom1.Value.Substring( 3 ) );
                                                else
                                                {
                                                    count = 1;
                                                }
                                            }
                                            else
                                            {
                                                compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                    "incorrect type specified; only variants of int and float allowed" );
                                                isValid = false;
                                            }

                                            if ( isValid )
                                            {
                                                // First, clear out any offending auto constants
                                                if ( named )
                                                { /*parameters->clearNamedAutoConstant(name);*/}
                                                else
                                                { /*parameters->clearAutoConstant(index);*/}

                                                int roundedCount = count % 4 != 0 ? count + 4 - ( count % 4 ) : count;
                                                if ( type == GpuProgramParameters.ElementType.Int )
                                                {
                                                    int[] vals = new int[ roundedCount ];
                                                    if ( getInts( prop.Values, 2, out vals, roundedCount ) )
                                                    {
                                                        try
                                                        {
                                                            if ( named )
                                                            { /*parameters.SetNamedConstant(name, vals, count, 1);*/}
                                                            else
                                                            { /*parameters.SetNamedConstant(index , vals, roundedCount/4);*/}
                                                        }
                                                        catch
                                                        {
                                                            compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                                "setting of constant failed" );
                                                        }
                                                    }
                                                    else
                                                    {
                                                        compiler.AddError( CompileErrorCode.NumberExpected, prop.File, prop.Line,
                                                            "incorrect integer constant declaration" );
                                                    }
                                                }
                                                else
                                                {
                                                    float[] vals = new float[ roundedCount ];
                                                    if ( getFloats( prop.Values, 2, out vals, roundedCount ) )
                                                    {
                                                        try
                                                        {
                                                            if ( named )
                                                            { /*parameters.SetNamedConstant(name, vals, count, 1);*/}
                                                            else
                                                            { /*parameters.SetNamedConstant(index , vals, roundedCount/4);*/}
                                                        }
                                                        catch
                                                        {
                                                            compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                                "setting of constant failed" );
                                                        }
                                                    }
                                                    else
                                                    {
                                                        compiler.AddError( CompileErrorCode.NumberExpected, prop.File, prop.Line,
                                                            "incorrect float constant declaration" );
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                            "param_named and param_indexed properties requires at least 3 arguments" );
                                    }
                                }
                                break;
                            #endregion ID_PARAM_INDEXED || ID_PARAM_NAMED

                            #region ID_PARAM_INDEXED_AUTO || ID_PARAM_NAMED_AUTO
                            case Keywords.ID_PARAM_INDEXED_AUTO:
                            case Keywords.ID_PARAM_NAMED_AUTO:
                                {
                                    bool named = ( prop.Id == (uint)Keywords.ID_PARAM_NAMED_AUTO );
                                    string name = string.Empty;
                                    int index = 0;

                                    if ( prop.Values.Count >= 2 )
                                    {
                                        AbstractNode i0 = getNodeAt( prop.Values, 0 ),
                                            i1 = getNodeAt( prop.Values, 1 ), i2 = getNodeAt( prop.Values, 2 ), i3 = getNodeAt( prop.Values, 3 );

                                        if ( i0.Type != AbstractNodeType.Atom || i1.Type != AbstractNodeType.Atom )
                                        {
                                            compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                "name or index and auto constant type expected" );
                                            return;
                                        }
                                        AtomAbstractNode atom0 = (AtomAbstractNode)i0, atom1 = (AtomAbstractNode)i1;
                                        if ( !named && !atom0.IsNumber )
                                        {
                                            compiler.AddError( CompileErrorCode.NumberExpected, prop.File, prop.Line,
                                                "parameter index expected" );
                                            return;
                                        }

                                        if ( named )
                                            name = atom0.Value;
                                        else
                                            index = int.Parse( atom0.Value );

                                        // Look up the auto constant
                                        atom1.Value = atom1.Value.ToLower();
                                        GpuProgramParameters.AutoConstantDefinition def = new GpuProgramParameters.AutoConstantDefinition(); //= GpuProgramParameters::getAutoConstantDefinition(atom1->value);

                                        //if ( def != null )
                                        //{
                                        switch ( def.DataType )
                                        {
                                            case GpuProgramParameters.AutoConstantDataType.None:
                                                // Set the auto constant
                                                try
                                                {
                                                    if ( named )
                                                    { /*parameters.SetNamedAutoConstant(name, def.AutoConstantType);*/}
                                                    else
                                                    { /*parameters.SetAutoConstant(index, def.AutoConstantType);*/}
                                                }
                                                catch
                                                {
                                                    compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                        "setting of constant failed" );
                                                }
                                                break;

                                            case GpuProgramParameters.AutoConstantDataType.Int:
                                                if ( def.AutoConstantType == GpuProgramParameters.AutoConstantType.AnimationParametric )
                                                {
                                                    try
                                                    {
                                                        if ( named )
                                                            parameters.SetNamedAutoConstant( name, def.AutoConstantType, animParametricsCount++ );
                                                        else
                                                            parameters.SetAutoConstant( index, def.AutoConstantType, animParametricsCount++ );
                                                    }
                                                    catch
                                                    {
                                                        compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                            "setting of constant failed" );
                                                    }
                                                }
                                                else
                                                {
                                                    // Only certain texture projection auto params will assume 0
                                                    // Otherwise we will expect that 3rd parameter
                                                    if ( i2 == prop.Values[ prop.Values.Count - 1 ] )
                                                    {
                                                        if ( def.AutoConstantType == GpuProgramParameters.AutoConstantType.TextureViewProjMatrix ||
                                                            def.AutoConstantType == GpuProgramParameters.AutoConstantType.TextureWorldViewProjMatrix ||
                                                            def.AutoConstantType == GpuProgramParameters.AutoConstantType.SpotLightViewProjMatrix ||
                                                            def.AutoConstantType == GpuProgramParameters.AutoConstantType.SpotLightWorldViewProjMatrix )
                                                        {
                                                            try
                                                            {
                                                                if ( named )
                                                                    parameters.SetNamedAutoConstant( name, def.AutoConstantType, 0 );
                                                                else
                                                                    parameters.SetAutoConstant( index, def.AutoConstantType, 0 );
                                                            }
                                                            catch
                                                            {
                                                                compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                                    "setting of constant failed" );
                                                            }
                                                        }
                                                        else
                                                        {
                                                            compiler.AddError( CompileErrorCode.NumberExpected, prop.File, prop.Line,
                                                                "extra parameters required by constant definition " + atom1.Value );
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bool success = false;
                                                        int extraInfo = 0;
                                                        if ( i3 == prop.Values[ prop.Values.Count - 1 ] )
                                                        { // Handle only one extra value
                                                            if ( getInt( i2, out extraInfo ) )
                                                            {
                                                                success = true;
                                                            }
                                                        }
                                                        else
                                                        { // Handle two extra values
                                                            int extraInfo1 = 0, extraInfo2 = 0;
                                                            if ( getInt( i2, out extraInfo1 ) && getInt( i3, out extraInfo2 ) )
                                                            {
                                                                extraInfo = extraInfo1 | ( extraInfo2 << 16 );
                                                                success = true;
                                                            }
                                                        }

                                                        if ( success )
                                                        {
                                                            try
                                                            {
                                                                if ( named )
                                                                    parameters.SetNamedAutoConstant( name, def.AutoConstantType, extraInfo );
                                                                else
                                                                    parameters.SetAutoConstant( index, def.AutoConstantType, extraInfo );
                                                            }
                                                            catch
                                                            {
                                                                compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                                    "setting of constant failed" );
                                                            }
                                                        }
                                                        else
                                                        {
                                                            compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                                "invalid auto constant extra info parameter" );
                                                        }
                                                    }
                                                }
                                                break;

                                            case GpuProgramParameters.AutoConstantDataType.Real:
                                                if ( def.AutoConstantType == GpuProgramParameters.AutoConstantType.Time ||
                                                    def.AutoConstantType == GpuProgramParameters.AutoConstantType.FrameTime )
                                                {
                                                    Real f = 1.0f;
                                                    if ( i2 != prop.Values[ prop.Values.Count - 1 ] )
                                                        getReal( i2, out f );

                                                    try
                                                    {
                                                        if ( named )
                                                        { /*parameters->setNamedAutoConstantReal(name, def->acType, f);*/}
                                                        else
                                                        { /*parameters->setAutoConstantReal(index, def->acType, f);*/}
                                                    }
                                                    catch
                                                    {
                                                        compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                            "setting of constant failed" );
                                                    }
                                                }
                                                else
                                                {
                                                    if ( i2 != prop.Values[ prop.Values.Count - 1 ] )
                                                    {
                                                        Real extraInfo = 0.0f;
                                                        if ( getReal( i2, out extraInfo ) )
                                                        {
                                                            try
                                                            {
                                                                if ( named )
                                                                { /*parameters->setNamedAutoConstantReal(name, def->acType, extraInfo);*/}
                                                                else
                                                                { /*parameters->setAutoConstantReal(index, def->acType, extraInfo);*/}
                                                            }
                                                            catch
                                                            {
                                                                compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                                    "setting of constant failed" );
                                                            }
                                                        }
                                                        else
                                                        {
                                                            compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                                                "incorrect float argument definition in extra parameters" );
                                                        }
                                                    }
                                                    else
                                                    {
                                                        compiler.AddError( CompileErrorCode.NumberExpected, prop.File, prop.Line,
                                                            "extra parameters required by constant definition " + atom1.Value );
                                                    }
                                                }
                                                break;
                                        }
                                        //}
                                        //else
                                        //{
                                        //    compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line );
                                        //}
                                    }
                                    else
                                    {
                                        compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line );
                                    }
                                }
                                break;
                            #endregion ID_PARAM_INDEXED_AUTO || ID_PARAM_NAMED_AUTO

                            default:
                                compiler.AddError( CompileErrorCode.UnexpectedToken, prop.File, prop.Line,
                                    "token \"" + prop.Name + "\" is not recognized" );
                                break;
                        }
                    }
                }
            }

            protected void _translateGpuProgram( ScriptCompiler compiler, ObjectAbstractNode obj )
            {
                NameValuePairList customParameters = new NameValuePairList();
                string syntax = string.Empty, source = string.Empty;
                AbstractNode parameters = null;

                foreach ( AbstractNode i in obj.Children )
                {
                    if ( i.Type == AbstractNodeType.Property )
                    {
                        PropertyAbstractNode prop = (PropertyAbstractNode)i;
                        if ( prop.Id == (uint)Keywords.ID_SOURCE )
                        {
                            if ( prop.Values.Count != 0 )
                            {
                                if ( prop.Values[ 0 ].Type == AbstractNodeType.Atom )
                                    source = ( (AtomAbstractNode)prop.Values[ 0 ] ).Value;
                                else
                                    compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                        "source file expected" );
                            }
                            else
                            {
                                compiler.AddError( CompileErrorCode.StringExpected, prop.File, prop.Line,
                                    "source file expected" );
                            }
                        }
                        else if ( prop.Id == (uint)Keywords.ID_SYNTAX )
                        {
                            if ( prop.Values.Count != 0 )
                            {
                                if ( prop.Values[ 0 ].Type == AbstractNodeType.Atom )
                                    syntax = ( (AtomAbstractNode)prop.Values[ 0 ] ).Value;
                                else
                                    compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                        "syntax string expected" );
                            }
                            else
                            {
                                compiler.AddError( CompileErrorCode.StringExpected, prop.File, prop.Line,
                                    "syntax string expected" );
                            }
                        }
                        else
                        {
                            string name = prop.Name, value = string.Empty;
                            bool first = true;
                            foreach ( AbstractNode it in prop.Values )
                            {
                                if ( it.Type == AbstractNodeType.Atom )
                                {
                                    if ( !first )
                                        value += " ";
                                    else
                                        first = false;

                                    value += ( (AtomAbstractNode)it ).Value;
                                }
                            }
                            customParameters.Add( name, value );
                        }
                    }
                    else if ( i.Type == AbstractNodeType.Object )
                    {
                        if ( ( (ObjectAbstractNode)i ).Id == (uint)Keywords.ID_DEFAULT_PARAMS )
                            parameters = i;
                        else
                            _processNode( compiler, i );
                    }
                }

                if ( !GpuProgramManager.Instance.IsSyntaxSupported( syntax ) )
                {
                    compiler.AddError( CompileErrorCode.UnsupportedByRenderSystem, obj.File, obj.Line );
                    //Register the unsupported program so that materials that use it know that
                    //it exists but is unsupported
                    GpuProgram unsupportedProg = GpuProgramManager.Instance.Create( obj.Name, compiler.ResourceGroup,
                        _translateIDToGpuProgramType( obj.Id ), syntax );

                    return;
                }

                // Allocate the program
                GpuProgram prog = null;
                //CreateGpuProgramScriptCompilerEvent evt(obj->file, obj->name, compiler->getResourceGroup(), source, syntax, _translateIDToGpuProgramType(obj.Id));
                bool processed = false;// compiler->_fireEvent(&evt, (void*)&prog);
                if ( !processed )
                {
                    prog = (GpuProgram)GpuProgramManager.Instance.CreateProgram( obj.Name, compiler.ResourceGroup, source,
                        _translateIDToGpuProgramType( obj.Id ), syntax );
                }

                // Check that allocation worked
                if ( prog == null )
                {
                    compiler.AddError( CompileErrorCode.ObjectAllocationError, obj.File, obj.Line,
                        "gpu program \"" + obj.Name + "\" could not be created" );
                    return;
                }

                obj.Context = prog;

                prog.IsMorphAnimationIncluded = false;
                //prog->setPoseAnimationIncluded(0);
                prog.IsSkeletalAnimationIncluded = false;
                prog.IsVertexTextureFetchRequired = false;
                prog.Origin = obj.File;

                // Set the custom parameters
                prog.SetParameters( customParameters );

                // Set up default parameters
                if ( prog.IsSupported && customParameters != null )
                {
                    throw new NotImplementedException();
                    //GpuProgramParametersShared ptr = prog.DefaultParameters;
                    //GpuProgramTranslator.TranslateProgramParameters( compiler, ptr, (ObjectAbstractNode)parameters );
                }
            }

            protected void _translateHighLevelGpuProgram( ScriptCompiler compiler, ObjectAbstractNode obj )
            {
                if ( obj.Values.Count == 0 )
                {
                    compiler.AddError( CompileErrorCode.StringExpected, obj.File, obj.Line );
                    return;
                }

                string language;
                if ( !getString( obj.Values[ 0 ], out language ) )
                {
                    compiler.AddError( CompileErrorCode.InvalidParameters, obj.File, obj.Line );
                    return;
                }

                NameValuePairList customParameters = new NameValuePairList();
                string source = string.Empty;
                AbstractNode parameters = null;

                foreach ( AbstractNode i in obj.Children )
                {
                    if ( i.Type == AbstractNodeType.Property )
                    {
                        PropertyAbstractNode prop = (PropertyAbstractNode)i;
                        if ( prop.Id == (uint)Keywords.ID_SOURCE )
                        {
                            if ( prop.Values.Count != 0 )
                            {
                                if ( prop.Values[ 0 ].Type == AbstractNodeType.Atom )
                                    source = ( (AtomAbstractNode)prop.Values[ 0 ] ).Value;
                                else
                                    compiler.AddError( CompileErrorCode.InvalidParameters, prop.File, prop.Line,
                                        "source file expected" );
                            }
                            else
                            {
                                compiler.AddError( CompileErrorCode.StringExpected, prop.File, prop.Line,
                                    "source file expected" );
                            }
                        }
                        else
                        {
                            string name = prop.Name, value = string.Empty;
                            bool first = true;
                            foreach ( AbstractNode it in prop.Values )
                            {
                                if ( it.Type == AbstractNodeType.Atom )
                                {
                                    if ( !first )
                                        value += " ";
                                    else
                                        first = false;

                                    if ( prop.Name == "attach" )
                                    {
                                        throw new NotImplementedException();
                                        string evtName = string.Empty;
                                        //ProcessResourceNameScriptCompilerEvent evt(ProcessResourceNameScriptCompilerEvent::GPU_PROGRAM, ((AtomAbstractNode*)(*it).get())->value);
                                        //compiler->_fireEvent(&evt, 0);
                                        value += evtName;
                                    }
                                    else
                                    {
                                        value += ( (AtomAbstractNode)it ).Value;
                                    }
                                }
                            }
                            customParameters.Add( name, value );
                        }
                    }
                    else if ( i.Type == AbstractNodeType.Object )
                    {
                        if ( ( (ObjectAbstractNode)i ).Id == (uint)Keywords.ID_DEFAULT_PARAMS )
                            parameters = i;
                        else
                            _processNode( compiler, i );
                    }
                }

                // Allocate the program
                HighLevelGpuProgram prog = null;

                throw new NotImplementedException();
                //CreateHighLevelGpuProgramScriptCompilerEvent evt(obj->file, obj->name, compiler->getResourceGroup(), source, language, 
                //    translateIDToGpuProgramType(obj->id));
                bool processed = false; // = compiler->_fireEvent(&evt, (void*)&prog);
                if ( !processed )
                {
                    prog = (HighLevelGpuProgram)(
                        HighLevelGpuProgramManager.Instance.CreateProgram( obj.Name, compiler.ResourceGroup, language, _translateIDToGpuProgramType( obj.Id ) ) );

                    prog.SourceFile = source;
                }

                // Check that allocation worked
                if ( prog == null )
                {
                    compiler.AddError( CompileErrorCode.ObjectAllocationError, obj.File, obj.Line,
                        "gpu program \"" + obj.Name + "\" could not be created" );
                    return;
                }

                obj.Context = prog;

                prog.IsMorphAnimationIncluded = false;
                //prog->setPoseAnimationIncluded(0);
                prog.IsSkeletalAnimationIncluded = false;
                prog.IsVertexTextureFetchRequired = false;
                prog.Origin = obj.File;

                // Set the custom parameters
                prog.SetParameters( customParameters );

                // Set up default parameters
                if ( prog.IsSupported && parameters != null )
                {
                    throw new NotImplementedException();
                    //GpuProgramParametersSharedPtr ptr = prog->getDefaultParameters();
                    //GpuProgramTranslator::translateProgramParameters(compiler, ptr, reinterpret_cast<ObjectAbstractNode*>(params.get()));
                }
            }

            protected void _translateUnifiedGpuProgram( ScriptCompiler compiler, ObjectAbstractNode obj )
            {
                NameValuePairList customParameters = new NameValuePairList();
                AbstractNode parameters = null;

                foreach ( AbstractNode i in obj.Children )
                {
                    if ( i.Type == AbstractNodeType.Property )
                    {
                        PropertyAbstractNode prop = (PropertyAbstractNode)i;
                        if ( prop.Name == "delegate" )
                        {
                            string value = string.Empty;
                            if ( prop.Values.Count != 0 && prop.Values[ 0 ].Type == AbstractNodeType.Atom )
                                value = ( (AtomAbstractNode)prop.Values[ 0 ] ).Value;

                            throw new NotImplementedException();
                            string evtName = string.Empty;
                            //ProcessResourceNameScriptCompilerEvent evt(ProcessResourceNameScriptCompilerEvent::GPU_PROGRAM, value);
                            //compiler->_fireEvent(&evt, 0);
                            customParameters.Add( "delegate", evtName );
                        }
                        else
                        {
                            string name = prop.Name, value = string.Empty;
                            bool first = true;
                            foreach ( AbstractNode it in prop.Values )
                            {
                                if ( it.Type == AbstractNodeType.Atom )
                                {
                                    if ( !first )
                                        value += " ";
                                    else
                                        first = false;
                                    value += ( (AtomAbstractNode)it ).Value;
                                }
                            }
                            customParameters.Add( name, value );
                        }
                    }
                    else if ( i.Type == AbstractNodeType.Object )
                    {
                        if ( ( (ObjectAbstractNode)i ).Id == (uint)Keywords.ID_DEFAULT_PARAMS )
                            parameters = i;
                        else
                            _processNode( compiler, i );
                    }
                }

                throw new NotImplementedException();

                // Allocate the program
                HighLevelGpuProgram prog = null;
                //CreateHighLevelGpuProgramScriptCompilerEvent evt(obj->file, obj->name, compiler->getResourceGroup(), "", "unified", translateIDToGpuProgramType(obj->id));
                bool processed = false;// compiler->_fireEvent(&evt, (void*)&prog);

                if ( !processed )
                {
                    prog = (HighLevelGpuProgram)(
                        HighLevelGpuProgramManager.Instance.CreateProgram( obj.Name, compiler.ResourceGroup, "unified", _translateIDToGpuProgramType( obj.Id ) ) );
                }

                // Check that allocation worked
                if ( prog == null )
                {
                    compiler.AddError( CompileErrorCode.ObjectAllocationError, obj.File, obj.Line,
                        "gpu program \"" + obj.Name + "\" could not be created" );
                    return;
                }

                obj.Context = prog;

                prog.IsMorphAnimationIncluded = false;
                //prog->setPoseAnimationIncluded(0);
                prog.IsSkeletalAnimationIncluded = false;
                prog.IsVertexTextureFetchRequired = false;
                prog.Origin = obj.File;

                // Set the custom parameters
                prog.SetParameters( customParameters );

                // Set up default parameters
                if ( prog.IsSupported && parameters != null )
                {
                    throw new NotImplementedException();
                    //GpuProgramParametersSharedPtr ptr = prog->getDefaultParameters();
                    //GpuProgramTranslator::translateProgramParameters(compiler, ptr, reinterpret_cast<ObjectAbstractNode*>(params.get()));
                }
            }
        }
    }
}
