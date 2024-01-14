using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Globalization;

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
                curve.curve.AddKey( float.Parse( lineEntry[0], CultureInfo.InvariantCulture.NumberFormat ), float.Parse( lineEntry[1], CultureInfo.InvariantCulture.NumberFormat ) );
            }

            ctx.AddObjectToAsset( fileName, textAsset );
            ctx.AddObjectToAsset( fileName, curve );
            ctx.SetMainObject( curve );
        }
    }
}