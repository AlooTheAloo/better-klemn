using Nova;
using NovaSamples.UIControls;
using Riptide.Transports;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chroma.Editor
{
    public struct CurrentlyPressedKey
    {
        public ushort player;
        public ushort allee;

        public CurrentlyPressedKey(ushort player, ushort allee)
        {
            this.player = player;
            this.allee = allee;
        }
    }

    public class EditorRecordInputManager : MonoBehaviour
    {
        [Header("Record button")]
        [SerializeField]
        private UIBlock2D recordButton;

        [SerializeField]
        private UIBlock2D recordStopButton;

        [SerializeField]
        private EditorTimeManager timeManager;

        [SerializeField]
        private EditorAlleyManager alleyManager;

        private Dictionary<CurrentlyPressedKey, float> touchesAppuyes = new();

        public bool isRecording;

        public void ToggleRecord() // appellé par le bouton record
        {
            timeManager.TogglePlayState();
            isRecording = timeManager.isPlaying;

            if (isRecording)
            {
                recordButton.gameObject.SetActive(false);
                recordStopButton.gameObject.SetActive(true);
            }
            else
            {
                recordStopButton.gameObject.SetActive(false);
                recordButton.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            isRecording = false;
        }

        private void Update()
        {
            if (!isRecording)
                return;

            ReadInputs();
        }

        private void ReadInputs()
        {
            for (int joueur = 0; joueur < Constantes.NOMBRE_JOUEURS; joueur++)
            {
                for (int allee = 0; allee < Constantes.NOMBRE_TOUCHES; allee++)
                {
                    var touche = Constantes.TOUCHES[joueur, allee];
                    var currentlyPressedKey = new CurrentlyPressedKey((ushort)joueur, (ushort)allee);
                    if (Input.GetKeyDown(touche) && !touchesAppuyes.ContainsKey(currentlyPressedKey))
                    {
                        touchesAppuyes.Add(
                        currentlyPressedKey,
                        timeManager.editorTime);
                        
                    }
                    if (Input.GetKeyUp(touche) && touchesAppuyes.ContainsKey(currentlyPressedKey))
                    {
                        var tempsDebut = touchesAppuyes[new((ushort)joueur, (ushort)allee)];
                        var duree = timeManager.editorTime - tempsDebut;

                        
                        if (EditorAlleyManager.SecondesToBeats(duree, EditorMapManager.MapData.MapVisualData.mapMetadonnees.bpm) >= 0.5f)
                        {
                            Debug.Log($"allee for hold note {allee}");
                            alleyManager.AddHoldNoteFromCopy(
                                tempsDebut,
                                (ushort)(joueur + 1),
                                (ushort)(allee + 1),
                                duree,
                                new()
                            );
                        }
                        else
                        {
                            Debug.Log($"allee for click note {allee}");

                            alleyManager.AddClickNoteFromCopy(
                                tempsDebut,
                                (ushort)(joueur + 1),
                                (ushort)(allee + 1),
                                new()
                            );
                        }
                        

                        touchesAppuyes.Remove(currentlyPressedKey);
                    }
                }
            }
        }
    }
}
