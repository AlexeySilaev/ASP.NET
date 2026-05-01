import React from 'react'
import { useState } from "react";
import axios from 'axios';
import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import heroImg from './assets/hero.png'
import CatFactList from './components/CatFactList'
import Loading from './components/Loading'
import Error from './components/Error'
import './App.css'

function App() {
    const [facts, setFacts] = useState([]);
    const [showFacts, setshowFacts] = useState(false);
    const [showLoading, setshowLoading] = useState(false);
    const [error, setError] = useState(null);

    const handleLoadCatFacts = async () => {
        if (showLoading) return;

        setError(null);
        setshowLoading(true);
        console.log("Обработка загрузки фактов.");
        setshowFacts(false);
        try {
            const response = await axios.get("https://catfact.ninja/facts");
            console.log(response);

            setshowLoading(false);
            if (response.status / 100 == 2) {
                const facts = response.data.data;
                setFacts(facts);
                setshowFacts(true);
                console.log("Факты загружены.");
            }
            else {
                setError(response.statusText);
                console.log("Произошла ошибка.");
            }
        }
        catch (error) {
            setshowLoading(false);
            setError(error.message);
            console.log("Произошла ошибка.");
        }
    }

  return (
    <>
      <section id="center">
        <div>
          <img src={heroImg} className="base" width="170" height="179" alt="" />
          <img src={reactLogo} className="framework" alt="React logo" />
          <img src={viteLogo} className="vite" alt="Vite logo" />
        </div>
        <h1>Факты о кошечках</h1>
        <button
          type="button"
          className="factloader"
             onClick={() => handleLoadCatFacts()}
        >
          Загрузить факты
              </button>
              {showLoading ? Loading() : null}
              {showFacts ? CatFactList(facts) : null}
              {error != null ? Error(error) : null}
      </section>
    </>
  )
}

export default App
