using Nova;
using NovaSamples.UIControls;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chroma.Editor
{
    [Serializable]
    public struct BlockButton
    {
        [SerializeField] internal UIBlock2D Panel;
        [SerializeField] internal UIBlock2D Bouton;
    }

    [Serializable]
    public struct EditorPanels
    {
        [SerializeField] internal BlockButton MainPanel;
        [SerializeField] internal BlockButton LumieresPanel;
        [SerializeField] internal BlockButton ParametresPanel;
    }

    public class ActivePanelManager : MonoBehaviour
    {
        [SerializeField] private Color couleurBoutonDesactive;

        [SerializeField] private Color couleurBoutonInactif;
        [SerializeField] private Color couleurBoutonActif;
        [SerializeField] EditorPanels panneaux;
        private FieldInfo[] panneauxFields;
        [SerializeField] private UIBlock2D panneauTop;

        private bool editeurBarre;
        public static event Action<BlockButton, EditorPanels> OnPanelChange;
        
        public bool PanneauTopActif
        {
            get
            {
                return panneauTop.gameObject.activeSelf;
            }
            set
            {
                throw new InvalidOperationException("Imposisble de changer PanneauTopActif au runtime.");
            }
        }

        public void SetPanneauActif(BlockButton panel)
        {
            if (editeurBarre) return;

            foreach (var panneau in panneauxFields.Select(x => (BlockButton) x.GetValue(panneaux)))
            {
                panneau.Panel.gameObject.SetActive(false);
                panneau.Bouton.Color = couleurBoutonInactif;
            }

            panel.Panel.gameObject.SetActive(true);
            panel.Bouton.Color = couleurBoutonActif;
            panneauTop.gameObject.SetActive(panel.Equals(panneaux.MainPanel));
            OnPanelChange?.Invoke(panel, panneaux);
        }
        
        private void Awake()
        {
            panneauxFields = panneaux.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void Start()
        {
            SetPanneauActif(EditorMapManager.MapData.MapVisualData.mapMetadonnees.titre != null
                ? panneaux.MainPanel
                : panneaux.ParametresPanel);

            foreach (var panneau in panneauxFields.Select(x => (BlockButton)x.GetValue(panneaux)))
            {
                Button b = panneau.Bouton.GetComponent<Button>();
                b.OnClicked.AddListener(() => { SetPanneauActif(panneau); });
            }
        }

        private void SwitchToPlay()
        {
            SetPanneauActif(panneaux.MainPanel);
        }

        private void OnEditeurBarreChanged(bool barre)
        {

            if (barre)
            {
                SetPanneauActif(panneaux.ParametresPanel);
                panneaux.MainPanel.Bouton.Color = couleurBoutonDesactive;
                panneaux.LumieresPanel.Bouton.Color = couleurBoutonDesactive;
            }
            editeurBarre = barre;
        }

        private void DetermineCorrectPanel(int panelNumber)
        {
            switch (panelNumber)
            {
                case 1 :
                    SetPanneauActif(panneaux.MainPanel);
                    break;
                case 2:
                    SetPanneauActif(panneaux.LumieresPanel);
                    break;
                case 3:
                    SetPanneauActif(panneaux.ParametresPanel);
                    break;
            }
        }

        private void OnEnable()
        {
            EditorMapManager.OnEditeurBarreChanged += OnEditeurBarreChanged;
            EditorHotkeys.swapComboPressed += DetermineCorrectPanel;
        }

        private void OnDisable()
        {
            EditorMapManager.OnEditeurBarreChanged -= OnEditeurBarreChanged;
            EditorHotkeys.swapComboPressed -= DetermineCorrectPanel;
        }
    }
}