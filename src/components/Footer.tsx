import styles from '../styles/Footer.module.css';
import github from '../images/github.png';
import google from '../images/google.png';
import { Link, useNavigate } from 'react-router-dom';



function Footer() {
    return (


        <div id={styles["ConteneurFooter"]}>
            {/* Footer */}

            <div id={styles["FooterHaut"]}>
                {/* Footer section haut*/}

                <div id={styles["SectionGauche"]}>
                    {/* Section Gauche */}
                    <h1>KLEMN</h1>

                    <div>

                        <h3>Curabitur id vestibulum nulla.
                            Phasellus vehicula sem odio, non posuere tortor porta at.
                            Integer tempor rutrum lorem, sed sagittis lacus commodo ac.
                            Nullam non ex a ligula ornare eleifend.
                            In vestibulum mollis nulla sed semper.
                            Sed bibendum, nunc sed tincidunt feugiat, nisl nisl bibendum diam,
                            at imperdiet lacus lacus vitae lacus. Proin a porttitor dui. </h3>

                    </div>
                </div>

                <div id={styles["SectionDroite"]}>
                    {/* Section Droite */}

                    <div className={styles.column}>
                        {/* Col 1 */}
                        <div>
                            {/* Titre col 1 */}
                            <h2>Produits</h2>
                        </div>
                        <div>
                            {/* Éléments col 1 */}
                            <div>
                                <Link to={'/apropos'} className='link'>
                                    <h3>À propos</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Resources</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Projet</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Entreprise</h3>
                                </Link>
                            </div>
                        </div>

                    </div>

                    <div className={styles.column}>
                        {/* Col 2 */}
                        <div>
                            {/* Titre col 2 */}
                            <h2>Platformes</h2>
                        </div>
                        <div>
                            {/* Éléments col 2 */}
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>API</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Github</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>React</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Oracle</h3>
                                </Link>
                            </div>
                        </div>
                    </div>

                    <div className={styles.column}>
                        {/* Col 3 */}
                        <div >
                            {/* Titre col 3 */}
                            <h2>Support</h2>
                        </div>
                        <div>
                            {/* Éléments col 3 */}
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Documentation</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Forum</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Services</h3>
                                </Link>
                            </div>
                            <div>
                                <Link to={'/forum'} className='link'>
                                    <h3>Status</h3>
                                </Link>
                            </div>
                        </div>
                    </div>

                </div>
            </div>

            <div id={styles["FooterBas"]}>
                {/* Footer section bas*/}
       

                <Link to={'/'} className='link' id={styles["TitreFooterBas"]}>
                    <h2>@2023 KLEMN, Inc.</h2>
                </Link>

                <div id={styles["FooterBasElements"]}>

                    <Link to={'/forum'} className='link'>
                        <h3>Termes d'utilisation</h3>
                    </Link>


                    <Link to={'/forum'} className='link'>
                        <h3>Confidentialité et sécurité</h3>
                    </Link>


                    <Link to={'/forum'} className='link'>
                        <h3>Qu'est-ce que KLEMN ?</h3>
                    </Link>
                </div>



                <div id={styles["SectionIcones"]}>
                    {/* Section icônes*/}

                    <div>
                        {/* Icône 1*/}
                        <img src={github} className='global_icon' width="30" height="30" alt="User" onClick={() => window.location.replace('https://github.com/')} />
                    </div>

                    <div>
                        {/* Icône 2*/}
                        <img src={google} className='global_icon' width="30" height="30" alt="User" onClick={() => window.location.replace('https://google.com/')} />
                    </div>

                </div>
            </div>
        </div>
    );
}

export default Footer;