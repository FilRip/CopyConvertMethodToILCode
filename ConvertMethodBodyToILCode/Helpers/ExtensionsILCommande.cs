using System.Collections.Generic;
using System.Linq;

using ConvertMethodBodyToILCode.Modeles;

namespace ConvertMethodBodyToILCode.Helpers
{
    internal static class ExtensionsILCommande
    {
        internal static ILCommande RetourneCommande(this List<ILCommande> listeILCommande, int offset)
        {
            return listeILCommande.SingleOrDefault(item => item.offset == offset);
        }

        internal static ILCommande RetourneCommande(this List<ILCommande> listeILCommande, byte offset)
        {
            return listeILCommande.SingleOrDefault(item => item.offset == offset);
        }

        internal static ILCommande RetourneCommande(this List<ILCommande> listeILCommande, sbyte offset)
        {
            return listeILCommande.SingleOrDefault(item => item.offset == offset);
        }
    }
}
