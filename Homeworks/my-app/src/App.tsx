import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import heroImg from './assets/hero.png'
import CatFactList from './components/CatFactList'
import './App.css'

function App() {
  return (
    <>
      <section id="center">
        <div className="hero">
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
        <div>
           <CatFactList />
        </div>
      </section>
    </>
  )

    const handleLoadCatFacts = async () => {
    }
}

export default App
