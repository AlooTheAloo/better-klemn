namespace Chroma.Server
{
    using Nova;
    using UnityEngine;

    public class ServerMenuManager : MonoBehaviour
    {
        [SerializeField] TextBlock IPTexte;
        [SerializeField] Transform confirmationTexte;

        private void OnEnable()
        {
            ServerManager.onGetIP += montrerIPMenu;
            ServerManager.onGetConnection += montrerConfirmation; // TODO : je pense pas qu'on a besoin de faire ca
        }

        private void OnDisable()
        {
            ServerManager.onGetIP -= montrerIPMenu;
            ServerManager.onGetConnection -= montrerConfirmation; // TODO : je pense pas qu'on a besoin de faire ca
        }

        private void montrerIPMenu(InterfaceInfo? info)
        {
            if (info != null)
            {
                IPTexte.Text = info.Value.IP + " sur l'interface " + info.Value.Name;
            } else
            {
                IPTexte.Text = "Interface trouvée" + info.Value.Name;
            }
        }
        private void montrerConfirmation()
        {
            confirmationTexte.gameObject.SetActive(true);
        }

    }

}
