using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;

using ConvertMethodBodyToILCode.Modeles;

namespace ConvertMethodBodyToILCode
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread()]
        static void Main()
        {
            MethodInfo mi = typeof(Program).GetMethod(nameof(MethodeTeste), BindingFlags.Static | BindingFlags.NonPublic);
            DynamicMethod dm = CopierMethode(mi, "MethodeTesteCopiee");
            dm.Invoke(null, null);
            Console.ReadKey();
        }

        private static int passage = 0;
        private static void MethodeTeste()
        {
            try
            {
                passage++;
                Console.WriteLine("This is a method");
                if (passage == 1)
                    Console.WriteLine("First pass");
                else
                    Console.WriteLine(passage.ToString() + " pass");
                throw new Exception("Fausse erreur");
            }
            catch (Exception ex) { Console.WriteLine("Catch block, ex : " + ex.Message); }
            finally { Console.WriteLine("Finally block"); }
        }

        internal static DynamicMethod CopierMethode(this MethodBase methodeACopier, string nomMethode = "")
        {
            if (string.IsNullOrWhiteSpace(nomMethode))
                nomMethode = methodeACopier.Name + "_Copy";

            // Copy return type (if not a void)
            Type typeDeRetour = typeof(void);
            if (methodeACopier is MethodInfo mi)
                typeDeRetour = mi.ReturnType;

            // Copy parameters to declare it in the 'copy' method
            // Note : if original method is instance (not static) or constructor : a parameter is add (at start) to specify instance (copied method is static)
            int nbParametres = methodeACopier.GetParameters().Length;
            if (!methodeACopier.IsStatic || methodeACopier is ConstructorInfo)
                nbParametres += 1;
            Type[] listeParametres = new Type[nbParametres];
            if (!methodeACopier.IsStatic || methodeACopier is ConstructorInfo)
            {
                if (methodeACopier is ConstructorInfo ci)
                    listeParametres[0] = ci.DeclaringType;
                else
                    listeParametres[0] = typeDeRetour;
                nbParametres++;
            }

            // Retreive type of parameters of original method
            foreach (ParameterInfo pi in methodeACopier.GetParameters())
                listeParametres[nbParametres++] = pi.ParameterType;

            // Copy method Body to list of IL Code
            List<ILCommande> listeOpCodes = methodeACopier.LireMethodBody();

            // Create new method
            DynamicMethod methodeCopiee = new(nomMethode, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeDeRetour, listeParametres, methodeACopier.DeclaringType, false);
            ILGenerator ilGen = methodeCopiee.GetILGenerator(methodeACopier.GetMethodBody().GetILAsByteArray().Length);

            // Add local variables of original method to the new method
            if (methodeACopier.GetMethodBody().LocalVariables != null && methodeACopier.GetMethodBody().LocalVariables.Count > 0)
                foreach (LocalVariableInfo lv in methodeACopier.GetMethodBody().LocalVariables)
                    ilGen.DeclareLocal(lv.LocalType, lv.IsPinned);

            // Declare list of label from original method to new method
            int nbLabels = listeOpCodes.Count(cmd => cmd.debutLabel);
            if (nbLabels > 0)
                for (int i = 0; i < nbLabels; i++)
                    ilGen.DefineLabel();

            // Finally copy method body
            foreach (ILCommande cmd in listeOpCodes)
            {
                cmd.Emit(ilGen);
            }

            return methodeCopiee;
        }
    }
}
