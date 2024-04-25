using ChromaWeb.Database;
using System;
using System.Diagnostics;
using UnityEngine;

public static class StaticStats
{
    public static TeamInfo equipe;

    public static ulong score = 0;
    public static float precision = 0;

    public static int[] meilleursCombos = new int[Constantes.NOMBRE_JOUEURS];

    public static int[] nombreNotesTotal = new int[Constantes.NOMBRE_JOUEURS];
    public static int[] nombreNotesParfait = new int[Constantes.NOMBRE_JOUEURS];
    public static int[] nombreNotesBien = new int[Constantes.NOMBRE_JOUEURS];
    public static int[] nombreNotesOk = new int[Constantes.NOMBRE_JOUEURS];
    public static int[] nombreNotesRatees = new int[Constantes.NOMBRE_JOUEURS];

    public static float[] precisions
    {
        get
        {
            float[] lesPrecisions = new float[Constantes.NOMBRE_JOUEURS];
            for (int i = 0; i < Constantes.NOMBRE_JOUEURS; i++)
            {
                if (nombreNotesTotal[i] == 0)
                {
                    lesPrecisions[i] = 100;
                }
                else
                {
                    float precisionJoueur = 0;

                    precisionJoueur +=
                        (ulong)nombreNotesParfait[i]
                        * Constantes.POINTS_PRECISION[(ushort)Precision.PARFAIT];
                    precisionJoueur +=
                        (ulong)nombreNotesBien[i]
                        * Constantes.POINTS_PRECISION[(ushort)Precision.BIEN];
                    precisionJoueur +=
                        (ulong)nombreNotesOk[i] * Constantes.POINTS_PRECISION[(ushort)Precision.OK];
                    precisionJoueur /= (ulong)nombreNotesTotal[i];

                    lesPrecisions[i] = MathF.Round(precisionJoueur, 2);
                }
            }
            return lesPrecisions;
        }
    }

    public static void Reset()
    {
        score = 0;
        precision = 0;

        meilleursCombos = new int[Constantes.NOMBRE_JOUEURS];
        nombreNotesTotal = new int[Constantes.NOMBRE_JOUEURS];
        nombreNotesParfait = new int[Constantes.NOMBRE_JOUEURS];
        nombreNotesBien = new int[Constantes.NOMBRE_JOUEURS];
        nombreNotesOk = new int[Constantes.NOMBRE_JOUEURS];
        nombreNotesRatees = new int[Constantes.NOMBRE_JOUEURS];
    }
}
