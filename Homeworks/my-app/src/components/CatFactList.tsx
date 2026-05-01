import React from 'react'
import { v4 as uuidv4 } from 'uuid';

function CatFactList(list) {
    console.log(list);

    return (

        <div style={{
            background: 'lightgreen',
            padding: '10px',
            borderRadius: '5px',
            fontSize: '12px',
            zIndex: 9999
        }}>
            <div>
                {list.map(item => (
                 <h2 key={uuidv4()}>
                    {item.fact}
                </h2>
            ))}
            </div>
        </div>
    )
}

export default CatFactList
