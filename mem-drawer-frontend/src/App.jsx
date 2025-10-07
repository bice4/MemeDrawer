import ImageGallery from "./ImageGallery"
import { Toast } from 'primereact/toast';
import { useState, useRef } from 'react';
import ImageDrawer from "./ImageDrawer";
import { Divider } from 'primereact/divider';


function App() {
  const toast = useRef(null);
  const [selectedImage, setSelectedImage] = useState(null);

  const selectImage = (img) => {
    setSelectedImage(img);
  }

  const showToast = (type, message) => {
    if (type === 1) {
      toast.current.show({
        severity: "error",
        summary: "Error",
        detail: message,
        life: 2000,
      });
    }

    if (type === 0) {
      toast.current.show({
        severity: "success",
        summary: "Success",
        detail: message,
        life: 2000,
      });
    }
  };


  return (
    <>
      <div className='flex align-items-center justify-content-center'>
        <ImageGallery onSelectImage={selectImage} onCallToast={showToast} />
      </div>

      {selectedImage && (
        <>
          <Divider />
          <ImageDrawer selectedImage={selectedImage} onCallToast={showToast} />
        </>
      )}

      <Toast ref={toast} />

    </>
  )
}

export default App
