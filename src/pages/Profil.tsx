import { useParams } from 'react-router';
import Bonjour from '../components/ComponentWithParameters';
import HelloWorldComponent from '../components/HelloWorldComponent';
import styles from '../styles/Profil.module.css'
import { SetStateAction, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PosteBlogue from '../components/Post/PosteBlogue';
import Post from '../components/Post';
import { auth } from '../firebase';
import toast from 'react-hot-toast'
import { onAuthStateChanged } from 'firebase/auth';
import FollowButton from '../components/FollowButton';
import { useAnimate } from 'framer-motion';

function Profil() {
    let { username } = useParams();
    const navigate = useNavigate();

    const [nom, setNom] = useState('');
    const [prenom, setPrenom] = useState('');
    const [displayName, setDisplayName] = useState('');
    const [dateCreationCompte, setDateCreationCompte] = useState('');
    const [nombreAbonnes, setNombreAbonnes] = useState(0);
    const [nombreAbonnesBefore, setNombreAbonnesBefore] = useState(nombreAbonnes);
    const [nombreAbonnements, setNombreAbonnements] = useState(0);
    const [bio, setBio] = useState('');
    const [urlImageProfil, setUrlImageProfil] = useState('');
    const [urlImageBanniere, setUrlImageBanniere] = useState('');
    const [visitorFollowsUser, setVisitorFollowsUser] = useState(false)

    const [idCompte, setIdCompte] = useState('')

    const [userPosts, setUserPosts] = useState<any[]>([])
    const [loadingPosts, setLoadingPosts] = useState(true)

    const [followerNumberScope, animateFollowerNumber] = useAnimate()


    useEffect(() => {
        // Unfollow
        if (nombreAbonnes < nombreAbonnesBefore) {
            animateFollowerNumber(followerNumberScope.current, {
                y: -20,
                opacity: 0,
                scale: 0.5,
            }, {
                duration: 0.1
            })
                .then(() => setNombreAbonnesBefore(nombreAbonnes))
                .then(() => {
                    animateFollowerNumber(followerNumberScope.current, {
                        y: [20, 0],
                        opacity: [0, 1],
                        scale: 1,
                    }, {
                        duration: 0.1
                    })
                })
        }

        // Follow
        else if (nombreAbonnes > nombreAbonnesBefore) {
            animateFollowerNumber(followerNumberScope.current, {
                y: 20,
                opacity: 0,
                scale: 0.5,
            }, {
                duration: 0.1
            })
                .then(() => setNombreAbonnesBefore(nombreAbonnes))
                .then(() => {
                    animateFollowerNumber(followerNumberScope.current, {
                        y: [-20, 0],
                        opacity: [0, 1],
                        scale: 1,
                    }, {
                        duration: 0.1
                    })
                })
        }
    }, [nombreAbonnes])


    useEffect(() => {
        onAuthStateChanged(auth, (user) => {
            fetch(`http://localhost:1111/profil`, {
                method: 'POST',
                body: JSON.stringify({
                    username: username,
                    is_followed_by: auth.currentUser?.uid
                }),
                headers: { 'Content-Type': 'application/json' }
            })
                .then(response => response.json())
                .then(response => {
                    let data = response

                    // if (!data) {
                    //     navigate("/404")
                    // }

                    setIdCompte(data.id_compte)
                    setNom(data.nom)
                    setPrenom(data.prenom)
                    setDisplayName(data.nom_affichage ? data.nom_affichage : username)
                    setDateCreationCompte(data.date_creation_compte)
                    setNombreAbonnes(data.nombre_abonnes)
                    setNombreAbonnements(data.nombre_abonnements)
                    setBio(data.biographie)
                    setUrlImageProfil(data.url_image_profil)
                    setUrlImageBanniere(data.url_image_banniere)
                    setVisitorFollowsUser(data.visitor_follows_profile)

                    return data.id_compte;

                }).then((userId) => {
                    fetch(`http://localhost:1111/user-posts/${userId}`, {
                        method: 'GET',
                        headers: { 
                            'Content-Type': 'application/json',
                        authorization: user?.uid || ""
                    }
                    }).then(response => response.json())
                        .then(response => {
                            let data = response;

                            setUserPosts(data);
                            setLoadingPosts(false);
                        })
                        .catch((error) => {
                            console.log(error)
                        })
                })
                .catch((error) => {
                    console.log(error)
                })
        });

    }, [username]);


    return (
        <div className={styles.flex}>

            <div className={styles.header}>
                <img className={styles.photo_banniere} src={urlImageBanniere} />

                <div className={styles.sous_banniere}>
                    <img className={styles.photo_profil} src={urlImageProfil} />

                    <div className={styles.infos_profil}>
                        <h2 className={styles.nom}>{displayName}</h2>
                        <p className={styles.username}>@{username}</p>
                        <FollowButton
                            userId={idCompte}
                            displayName={displayName}
                            nombreAbonnes={nombreAbonnes}
                            setNombreAbonnes={setNombreAbonnes}
                            visitorFollowsUser={visitorFollowsUser}
                            setVisitorFollowsUser={setVisitorFollowsUser} />
                    </div>

                </div>

                <div className={styles.follows}>
                    <div><p ref={followerNumberScope}>{nombreAbonnesBefore}</p> abonnés</div>
                    <div><p>{nombreAbonnements}</p> Abonnements</div>
                </div>

                <p className={styles.bio}>{bio}</p>
            </div>

            {loadingPosts && (
                <div>Chargement...</div>
            )}

            {userPosts?.map(({
                contenu,
                date_publication,
                id_compte,
                id_infos,
                id_parent,
                id_post,
                id_type_post,
                nombre_commentaires,
                nombre_dislikes,
                nombre_likes,
                nombre_partages,
                nombre_reposts,
                titre,
                url_image_profil,
                vote
            }) => {
                return (
                    <Post
                        idPost={id_post}
                        idCompte={id_compte}
                        date={date_publication}
                        nomAffichage={displayName}
                        nomUtilisateur={username + ''}
                        titre={titre}
                        contenu={contenu}
                        nombreLike={nombre_likes}
                        nombreDislike={nombre_dislikes}
                        nombrePartage={nombre_partages}
                        nombreCommentaire={nombre_commentaires}
                        type={id_type_post}
                        isPostFullScreen={false}
                        urlImageProfil={urlImageProfil}
                        userVote={vote} />
                )
            })}

        </div>
    );
}

export default Profil;
