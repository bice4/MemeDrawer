import { useState, useEffect, useRef } from "react";
import { InputText } from "primereact/inputtext";
import { ColorPicker } from 'primereact/colorpicker';
import { Divider } from 'primereact/divider';
import { Slider } from 'primereact/slider';
import { Button } from 'primereact/button';
import { Checkbox } from "primereact/checkbox";

export default function ImageDrawer({ selectedImage, onCallToast }) {

    const [topText, setTopText] = useState('');
    const [bottomText, setBottomText] = useState('');
    const [imageUrl, setImageUrl] = useState('');
    const [textColor, setTextColor] = useState("ffffff");
    const [backgroundColor, setBackgroundColor] = useState("000000");
    const [backgroundOpacity, setBackgroundOpacity] = useState(120);
    const [withOutline, setWithOutline] = useState(false);

    const apiUrl = '/api/ImageDrawer';
    const lastRequestRef = useRef({});

    const resetToDefault = () => {

        const tColor = "ffffff";
        const bColor = "000000";
        const bOpacity = 120;
        const uOutline = false;


        setBackgroundColor(bColor);
        setTextColor(tColor);
        setBackgroundOpacity(bOpacity);
        setWithOutline(uOutline);

        generateImage(topText, bottomText, tColor, bColor, bOpacity, uOutline);
    }

    const setTopTexxt = (e) => {
        const value = e;
        setTopText(value);
        generateImage(value, bottomText, textColor, backgroundColor, backgroundOpacity, withOutline);
    }

    const setBottomTexxt = (e) => {
        const value = e;
        setBottomText(value);
        generateImage(topText, value, textColor, backgroundColor, backgroundOpacity, withOutline);
    }

    const setTextColorr = (e) => {
        const value = e;
        setTextColor(e);
        generateImage(topText, bottomText, value, backgroundColor, backgroundOpacity, withOutline);
    }

    const setBackgroundColorr = (e) => {
        const value = e;
        setBackgroundColor(e);
        generateImage(topText, bottomText, textColor, value, backgroundOpacity, withOutline);
    }

    const setBackgroundOpacityy = (e) => {
        const value = e;
        setBackgroundOpacity(e);
        generateImage(topText, bottomText, textColor, backgroundColor, value, withOutline);
    }

    const setWithOutlinee = (e) => {
        const value = e;
        setWithOutline(e);
        generateImage(topText, bottomText, textColor, backgroundColor, backgroundOpacity, value);
    }

    const generateImage = async (top, bottom, textColorHex, backgoundColorHex, backgoundOpacity, withOutline) => {
        if (!top && !bottom) return;

        const current = {
            topText: top, bottomText: bottom, textColorHex: textColorHex, backgroundColorHex: backgoundColorHex,
            BackgroundOpacity: backgoundOpacity, withOutline: withOutline
        };
        if (JSON.stringify(current) === JSON.stringify(lastRequestRef.current)) {
            return;
        }

        lastRequestRef.current = current;

        const response = await fetch(apiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                imageId: selectedImage.id,
                ...current
            })
        });

        if (response.ok) {
            const blob = await response.blob();
            const imageUrl = URL.createObjectURL(blob);
            setImageUrl(imageUrl);
        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to generate image');
        }
    }
    useEffect(() => {
        setTopText('');
        setBottomText('');
        setImageUrl('');
    }, [selectedImage]);

    return (
        <div className="flex justify-content-center pt-3">
            <div className='col-3'>
                <div className='flex column'>
                    <InputText type="text" value={topText} onChange={(e) => setTopTexxt(e.target.value)} className="p-inputtext-lg" placeholder="Top text" />
                </div>
                <div className='flex column mt-5'>
                    <InputText type="text" value={bottomText} onChange={(e) => setBottomTexxt(e.target.value)} className="p-inputtext-lg" placeholder="Bottom text" />
                </div>
                <Divider />

                <div className='flex column mt-5'>
                    <div className="flex column  justify-content-left mt-2">
                        <label htmlFor="textColor" className="col-10 mt-1">Text color:</label>
                        <div className="col-3">
                            <ColorPicker id="textColor" value={textColor} onChange={(e) => setTextColorr(e.value)} />
                        </div>
                    </div>
                </div>
                <div className='flex column mt-2'>
                    <div className="flex column  justify-content-left mt-1">
                        <label htmlFor="backgoundColor" className="col-10 mt-1">Background color:</label>
                        <div className="col-3">
                            <ColorPicker id="backgoundColor" value={backgroundColor} onChange={(e) => setBackgroundColorr(e.value)} />
                        </div>
                    </div>
                </div>
                <div className='flex column mt-2'>
                    <div className="flex column  justify-content-left mt-1">
                        <label htmlFor="backgroundOpactiy" className="col-6 mt-1">Background opacity:</label>
                        <div className="col-3 mt-2">
                            <Slider id="backgroundOpactiy" value={backgroundOpacity} step={10} onChange={(e) => setBackgroundOpacityy(e.value)} min={0} max={254} className="w-10rem" />
                        </div>
                    </div>
                </div>
                <div className='flex column mt-2'>
                    <div className="flex column  justify-content-left mt-1">
                        <Checkbox inputId="uo" name="pizza" value="Use outline" onChange={e => setWithOutlinee(e.checked)} checked={withOutline} />
                        <label htmlFor="uo" className="ml-2">Outline</label>
                    </div>
                </div>
                <div className='flex column mt-2'>
                    <div className="flex column justify-content-center mt-1">
                        <div className="col-12">
                            <Button label="Reset to default" severity="secondary" text onClick={resetToDefault} />
                        </div>
                    </div>
                </div>
            </div>
            <div className='col-9'>
                {imageUrl ? (<img src={imageUrl} />) : (<img src={`data:image/png;base64,${selectedImage.base64Content}`} />)}
            </div>
        </div>
    );
}