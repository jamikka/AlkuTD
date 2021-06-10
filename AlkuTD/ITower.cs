using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlkuTD
{
    public enum DmgType { Basic, Splash, None }
    public enum UpgLvl { Basic, Advanced, Max }
    public enum ColorPriority { none, red, green, blue }
    public enum TargetPriority
    {
        none = 0,

        first = 1,
        last = 2,

        tough = 3,
        weak = 4,

        fast = 5,
        slow = 6,

        mob = 7,

        far = 8,
        close = 9
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
