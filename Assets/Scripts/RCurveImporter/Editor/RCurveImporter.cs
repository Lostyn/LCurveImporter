using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Globalization;
using System;

namespace fr.lostyn.RCurve
{
    [ScriptedImporter( 1, "rcurve" )]
    public class RCurveImporter : ScriptedImporter
    {
        public override void OnImportAsset( AssetImportContext ctx ) {
            string fileContent = File.ReadAllText( ctx.assetPath );
            var fileName = Path.GetFileNameWithoutExtension( ctx.assetPath );

            var textAsset = new TextAsset( fileContent );
            textAsset.name = $"{fileName}_raw";

            var curve = ScriptableObject.CreateInstance<RCurve>();
            curve.name = fileName;
            var aCurve = curve.curve;

            curve.curve = new AnimationCurve();

            string[] lines = fileContent.Split( '\n' );
            string[] lineEntry;
            for( int i = 0; i < lines.Length; i++ ) {
                lineEntry = lines[i].Split( " " );
                curve.curve.AddKey( ParseInput( lineEntry[0] ), ParseInput( lineEntry[1] ) );
            }

            ctx.AddObjectToAsset( fileName, textAsset );
            ctx.AddObjectToAsset( fileName, curve );
            ctx.SetMainObject( curve );
        }

        public float ParseInput(string input ) {
            if( input.Contains( "." ) )
                return Single.Parse( input, CultureInfo.InvariantCulture ) ;
            else 
                return Single.Parse( input);
        }
    }
}