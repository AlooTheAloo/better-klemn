using Nova;
using NovaSamples.UIControls;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Chroma.Editor
{
    public class LumieresEditeur : MonoBehaviour
    {
        const int AUCUN_GROUPE = -1;
        private bool bound = false;

        [Header("Couleurs")]
        [SerializeField] Color couleurSelectionne;
        [SerializeField] Color couleurDeselectionne;

        [Header("Refs Générales")]
        [SerializeField] ListView listeGroupes;
        [SerializeField] ListView listeLumieres;
        [SerializeField] GameObject groupOptionsPanel;

        [Header("Refs panneau groupes")]
        [SerializeField] GameObject modifierGroupePanel;
        [SerializeField] TextField fieldGroupe;
        [SerializeField] GameObject boutonModifierPanneauGroupe;

        [Header("Refs panneau lumières")]
        [SerializeField] GameObject modifierLumieresPanel;
        [SerializeField] UIBlock2D[] typesLumieres;
        [SerializeField] UIBlock2D[] projecteursLumieres;
        [SerializeField] TextField fieldNomLumiere;
        [SerializeField] MultiSelectDropdown groupesDropdown;
        [SerializeField] TextField[] etatPositions;
        [SerializeField] TextField[] etatTaille;
        [SerializeField] ColorSelector etatCouleur;
        [SerializeField] TextField etatOpacite;
        [SerializeField] UIBlock2D boutonModifier;


        private int indexGroupeSelectionne = AUCUN_GROUPE;
        internal List<string> groupes = new();
        internal List<LightObjectInitiaialisePacket> objetsLumiere = new();
        List<LightObjectInitiaialisePacket> objetsLumiereAffiches = new();
        
        public List<string> Groupes => groupes;

        int packetOuvertIndex;

        public void AddGroupe()
        {
            int oldIndex = indexGroupeSelectionne;
            groupes.Add("Nouveau groupe");
            indexGroupeSelectionne = groupes.Count - 1;
            Refresh(oldIndex, indexGroupeSelectionne);
            JumpTo(listeGroupes, indexGroupeSelectionne);
        }

        private void OnEnable()
        {
            EditorMapManager.OnMapChanged += OnMapChanged;
            ActivePanelManager.OnPanelChange += OnPanelChange;
            listeGroupes.AddDataBinder<string, EditorGroupVisuals>(BindGroupes);
            listeGroupes.AddGestureHandler<Gesture.OnClick, EditorGroupVisuals>(ClickGroupeItem);

            listeLumieres.AddDataBinder<LightObjectInitiaialisePacket, EditorLightObjectVisuals>(BindObjets);

            fieldGroupe.OnTextChanged += ModifierGroupePanelOnChange;

            fieldNomLumiere.OnTextChanged += VerifierModification;

            foreach(var pos in etatPositions)
                pos.OnTextChanged += VerifierModification;

            foreach (var taille in etatTaille)
                taille.OnTextChanged += VerifierModification;

            etatOpacite.OnTextChanged += VerifierModification;

            ItemLumiereEditeur.onDemandeModifier += OuvrirPanneauModificationObjet;
            ItemLumiereEditeur.onDemandeDupliquer += DupliquerObjetLumineux;

        }

        private void OnDisable()
        {
            EditorMapManager.OnMapChanged -= OnMapChanged;
            ActivePanelManager.OnPanelChange -= OnPanelChange;
            ItemLumiereEditeur.onDemandeModifier -= OuvrirPanneauModificationObjet;
            ItemLumiereEditeur.onDemandeDupliquer -= DupliquerObjetLumineux;

        }

        private void BindGroupes(Data.OnBind<string> evt, EditorGroupVisuals target, int index)
        {
            target.Bind(evt.UserData);
            if (index == indexGroupeSelectionne)
            {
                target.arrierePlan.Color = couleurSelectionne;
            }
            else target.arrierePlan.Color = couleurDeselectionne;
        }

        private void BindObjets(Data.OnBind<LightObjectInitiaialisePacket> evt, EditorLightObjectVisuals target, int index)
        {
            target.Bind(evt.UserData);
        }

        private void ClickGroupeItem(Gesture.OnClick evt, EditorGroupVisuals target, int index)
        {
            int oldIndex = indexGroupeSelectionne;
            indexGroupeSelectionne = index;
            Refresh(oldIndex, indexGroupeSelectionne);
        }

        public void OnMapChanged(EditorMap map)
        {
            if (map.ListeGroupes == null) return;
            groupes = map.ListeGroupes;
            objetsLumiere = map.ListeObjetLumieres;
        }

        public void OnPanelChange(BlockButton panneau, EditorPanels panneaux)
        {
            if (panneau.Equals(panneaux.LumieresPanel))
            {
                DataBind();
            }
        }

        public void DataBind()
        {
            if (bound) return;
            bound = true;
            listeGroupes.SetDataSource(groupes);
            listeLumieres.SetDataSource(objetsLumiereAffiches);
            Refresh(AUCUN_GROUPE, AUCUN_GROUPE);
        }

        public void Refresh(int oldIndex, int newIndex)
        {
            listeGroupes.Relayout();
            RebindElements(listeGroupes, oldIndex, newIndex);
            groupOptionsPanel.SetActive(indexGroupeSelectionne != AUCUN_GROUPE); // Si un groupe est sélectionné, on montre les options


            objetsLumiereAffiches = indexGroupeSelectionne == -1 ? objetsLumiere : objetsLumiere
                .Where(x => x.lumiereData.groupes
                .Contains(groupes[indexGroupeSelectionne]))
                .ToList();
            listeLumieres.SetDataSource(objetsLumiereAffiches);
            listeLumieres.Refresh();
        }

        public void DeselectionnerGroupe()
        {
            int oldGroup = indexGroupeSelectionne;
            indexGroupeSelectionne = AUCUN_GROUPE;
            Refresh(oldGroup, indexGroupeSelectionne);
        }

        public void RebindElements(ListView liste, params int[] indexes)
        {
            foreach (int index in indexes)
            {
                if (index == AUCUN_GROUPE) continue;
                liste.Rebind(index);
            }
        }

        public void JumpTo(ListView liste, int index)
        {
            if (index != AUCUN_GROUPE)
                liste.JumpToIndex(index);
        }

        public void ModifierGroupeClick()
        {
            modifierGroupePanel.SetActive(true);
            fieldGroupe.Text = groupes[indexGroupeSelectionne];
        }
        
        public void ModifierGroupePanelModifier()
        {
            string ancienNom = groupes[indexGroupeSelectionne];
            string nouveauNom = fieldGroupe.Text;
            groupes[indexGroupeSelectionne] = nouveauNom;
            objetsLumiere.ForEach(x =>
            {
                for (int i = 0; i < x.lumiereData.groupes.Length; i++)
                {
                    if (x.lumiereData.groupes[i] == ancienNom)
                    {
                        x.lumiereData.groupes[i] = nouveauNom;
                        break;
                    }
                }
            });
            
            Refresh(indexGroupeSelectionne, indexGroupeSelectionne);
            FermerModifierGroupePanel();
        }

        public void ModifierGroupePanelSupprimer()
        {
            groupes.RemoveAt(indexGroupeSelectionne);
            int oldIndex = indexGroupeSelectionne;
            indexGroupeSelectionne = AUCUN_GROUPE;
            listeGroupes.Refresh();
            Refresh(oldIndex, AUCUN_GROUPE);
            FermerModifierGroupePanel();
        }

        public void FermerModifierGroupePanel()
        {
            modifierGroupePanel.SetActive(false);
        }

        public void ModifierGroupePanelOnChange()
        {
            boutonModifierPanneauGroupe.SetActive(fieldGroupe.Text != string.Empty);
        }

        public void DupliquerObjetLumineux(LightObjectInitiaialisePacket obj)
        {
            var packet = obj.Clone();

            if (indexGroupeSelectionne != AUCUN_GROUPE)
                packet.lumiereData.groupes = new string[] { groupes[indexGroupeSelectionne] };

            objetsLumiere.Add(packet);

            Refresh(indexGroupeSelectionne, indexGroupeSelectionne);
        }

        public void OuvrirPanneauModificationObjet(LightObjectInitiaialisePacket packet)
        {
            packetOuvertIndex = objetsLumiere.IndexOf(packet);

            // Light Data
            modifierLumieresPanel.SetActive(true);
            fieldNomLumiere.Text = packet.lumiereData.nomLumiere;
            ActiverPanneauAIndex(typesLumieres, (int) packet.lumiereData.type);
            ActiverPanneauAIndex(projecteursLumieres, packet.lumiereData.projectorID);
            groupesDropdown.SetOptions(groupes, packet.lumiereData.groupes.ToList());


            // Light State
            foreach (var etat in packet.initialState.listeEtats)
            {
                if(etat is PositionLightState pls)
                {
                    etatPositions[0].Text = pls.targetPosition.x.ToString(CultureInfo.InvariantCulture);
                    etatPositions[1].Text = pls.targetPosition.y.ToString(CultureInfo.InvariantCulture);
                }
                else if (etat is ScaleLightState sls)
                {
                    etatTaille[0].Text = sls.taille.x.ToString(CultureInfo.InvariantCulture);
                    etatTaille[1].Text = sls.taille.y.ToString(CultureInfo.InvariantCulture);
                }
                else if (etat is ColorLightState cls)
                {
                    etatCouleur.SetColor(cls.lightColor) ;
                }
                else if (etat is OpacityLightState ols)
                {
                    etatOpacite.Text = (ols.opacity * 100f).ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public void ActiverPanneauAIndex(UIBlock2D[] panneaux, int index)
        {
            print("Activation du panneau a index " + index);
            foreach (var panneau in panneaux)
            {
                panneau.Color = couleurDeselectionne;
            }

            panneaux[index].Color = couleurSelectionne;
        }

        public int TrouverPanneauActif(UIBlock2D[] panneaux)
        {
            for(int i = 0; i < panneaux.Length; i++)
            {
                if (panneaux[i].Color == couleurSelectionne)
                {
                    return i;
                }
            }
            return -1;
        }

        public void ActiverCercle()
        {
            ActiverPanneauAIndex(typesLumieres, (int) ShapeActorType.ELLIPSE);
        }

        public void ActiverCarre()
        {
            ActiverPanneauAIndex(typesLumieres, (int)ShapeActorType.RECTANGLE);
        }

        public void ActiverProjecteurGauche()
        {
            ActiverPanneauAIndex(projecteursLumieres, 0);
        }

        public void ActiverProjecteurDroite()
        {
            ActiverPanneauAIndex(projecteursLumieres, 1);
        }

        public void PanneauObjetModifier()
        {
            var packet = objetsLumiere[packetOuvertIndex];
            packet.lumiereData.nomLumiere = fieldNomLumiere.Text;
            packet.lumiereData.projectorID = (ushort) TrouverPanneauActif(projecteursLumieres);
            packet.lumiereData.type = (ShapeActorType) TrouverPanneauActif(typesLumieres);
            packet.lumiereData.groupes = groupesDropdown.GetSelectedOptions().ToArray();

            LightState[] states = new LightState[]
            {
                new PositionLightState(new Vector2(float.Parse(etatPositions[0].Text), float.Parse(etatPositions[1].Text)), StateMeasurement.ABSOLUTE),
                new ScaleLightState(new Vector2(float.Parse(etatTaille[0].Text), float.Parse(etatTaille[1].Text)), StateMeasurement.ABSOLUTE),
                new ColorLightState(ColorUtility.ToHtmlStringRGB(etatCouleur.GetColor()), StateMeasurement.ABSOLUTE),
                new OpacityLightState(float.Parse(etatOpacite.Text) / 100f, StateMeasurement.ABSOLUTE)
            };
            
            print(etatCouleur.GetColor());

            packet.initialState = new LightObjectState(states);
            objetsLumiere[packetOuvertIndex] = packet;

            Refresh(packetOuvertIndex, packetOuvertIndex);
            PanneauObjetAnnuler();
        }

        public void PanneauObjetSupprimer()
        {
            objetsLumiere.RemoveAt(packetOuvertIndex);
            Refresh(indexGroupeSelectionne, indexGroupeSelectionne);
            PanneauObjetAnnuler();
        }

        public void PanneauObjetAnnuler()
        {
            modifierLumieresPanel.SetActive(false);
        }

        public void AjouterLaser()
        {
            var packet = new LightObjectInitiaialisePacket(new LightObjectState().Init(), new ObjetLumiereData().init());
            
            if(indexGroupeSelectionne != AUCUN_GROUPE)
                packet.lumiereData.groupes = new string[] { groupes[indexGroupeSelectionne] };
            
            objetsLumiere.Add(packet);
           
            Refresh(indexGroupeSelectionne, indexGroupeSelectionne);
        }

        private void VerifierModification()
        {
            bool valide = true;

            // Nom laser
            if (fieldNomLumiere.Text == string.Empty || fieldNomLumiere.Text.Contains(XmlConstants.GROUPS_SEPARATOR))
            {
                valide = false;
            }

            // Etat pos
            if (!float.TryParse(etatPositions[0].Text, out _) || !float.TryParse(etatPositions[1].Text, out _))
            {
                valide = false;
            }

            // Etat taille
            if (!float.TryParse(etatTaille[0].Text, out _) || !float.TryParse(etatTaille[1].Text, out _))
            {
                valide = false;
            }

            // Etat Opacite
            if (float.TryParse(etatOpacite.Text, out float opacite))
            {
                if (opacite < 0 || opacite > 100)
                {
                    valide = false;
                }
            }
            else
            {
                valide = false;
            }

            boutonModifier.gameObject.SetActive(valide);
        }
    }

}