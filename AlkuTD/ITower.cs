using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlkuTD
{
    public enum DmgType { Basic, Splash, None }
    public enum UpgLvl { Basic, Advanced, Max }
    public enum ColorPriority { None, Red, Green, Blue }
    public enum TargetPriority
    {
        None = 0,

        First = 1,
        Last = 2,

        Tough = 3,
        Weak = 4,

        Fast = 5,
        Slow = 6,

        Mob = 7,
        Far = 8
    }

    public enum TowerSymbol // NOT IN USE YET
    {
        A, E, I, O, U, X,
        Ä, Ë, Ï, Ö, Ü, Y,
        Â, Ê, Î, Ô, Û, Z
    }
    public interface ITower
    {
        void Disassemble();

        void Update(List<Creature> creatures);

        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);

        Tower Clone(Tower cloneSource);
    }
}
