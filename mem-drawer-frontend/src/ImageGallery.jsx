
import { ProgressSpinner } from 'primereact/progressspinner';
import { useState, useEffect } from "react";
import './ImageGallery.css';


export default function ImageGallery({ onSelectImage, onCallToast }) {

    const [isLoading, setIsLoading] = useState(true);
    const [images, setImages] = useState([]);

    const apiUrl = '/api';


    const getGeneratedImages = async () => {
        fetch(`${apiUrl}/Image`)
            .then(response => response.json())
            .then(json => {
                setIsLoading(false);
                setImages(json);
            })
            .catch(error => {
                console.error('Error fetching photos:', error);
                setIsLoading(false);
                onCallToast(1, 'Failed to fetch photos')
            });
    }

    useEffect(() => {
        getGeneratedImages();
    }, []);


    return (
        <div className="flex align-items-center justify-content-center pt-3">
            {isLoading ?
                (<ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="8" fill="var(--surface-ground)" animationDuration=".5s" />)
                : images.length === 0
                    ? (<div>No images found</div>)
                    : (< >
                        <div className="flex grid">
                            {images.map((img) => (
                                <div
                                    key={img.id}
                                    onClick={() => onSelectImage(img)}
                                    className='ml-2'
    
                                >
                                    <img
                                        src={`data:image/png;base64,${img.base64Content}`}
                                        alt={img.name}
                                        width="118"
                                        height="118"
                                        className='image-hover-zoom'
                                    />
                                </div>
                            ))}
                        </div>
                    </>)}
        </div>
    );
}