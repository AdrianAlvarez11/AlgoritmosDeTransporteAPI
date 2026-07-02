document.addEventListener('DOMContentLoaded', function() {

    const label_nav = document.getElementById('label_opciones');
    const nav = document.querySelector('nav');

    //////////////////////////////////////////
    //Menu superior
    window.addEventListener('click', function(e) {
        if (label_nav.contains(e.target)) {
            nav.classList.toggle('invisible');
        }
        else if (!nav.classList.contains('invisible')) {
            const clicEnOpcion = e.target.closest('button') || e.target.closest('a');
            const clicFueraDelNav = !nav.contains(e.target);

            if (clicEnOpcion || clicFueraDelNav) {
                nav.classList.add('invisible');
            }
        }
    });



    ///////////////////////////////////
    //Navegacion
    const vistaGuardada = sessionStorage.getItem('idVista');
    if (vistaGuardada){
        CambiarVista(vistaGuardada);
    }

    document.querySelectorAll('[data-section]').forEach(btn => {
        btn.addEventListener('click', () => {
            const idDestino = btn.getAttribute('data-section');

            CambiarVista(idDestino);
        });
    });

    function CambiarVista(idVista){
        const VistaDestino = document.getElementById(idVista);
        if (VistaDestino) {
            // Ocultar todas las secciones
            document.querySelectorAll('section').forEach(s => s.hidden = true);
            
            // Mostrar y guardar la sección con IdVista
            VistaDestino.hidden = false;
            sessionStorage.setItem('idVista', idVista);
        }
    }














    /////////////////////////////////
    //////////////////////////////////////////////
    //////////////////////////////////
    //Primer tabla
    //Creacion de celdas

    const slcFilas = document.getElementById("filas");
    const slcColumnas = document.getElementById("columnas");
    const btnCrear = document.getElementById("crearTabla");
    const tablaIngreso = document.getElementById("tabla_ingreso");
    const spnError = document.getElementById("error");

    
    btnCrear.addEventListener('click', function(e){
        spnError.textContent = "";
        //template de encabezado y celda

        const filas = Number(slcFilas.value);
        const columnas = Number(slcColumnas.value);
        
        if (filas < 2 || columnas < 2) {
            spnError.textContent = "Indica un valor de fila y columna mayor a 1";
            return;
        }

        tablaIngreso.innerHTML = '';


        const columnasTotalesGrid = columnas + 2; 
        tablaIngreso.style.gridTemplateColumns = `repeat(${columnasTotalesGrid}, 1fr)`;


        const tplHeader = document.getElementById("template_header_inicial");
        const tplCelda = document.getElementById("template_celda_input");

        // Función para clonar y colocar texto en encabezados
        function crearHeader(texto) {
            const clon = tplHeader.content.cloneNode(true);
            clon.querySelector('.texto-header').textContent = texto;
            return clon;
        }

        //Crear toda la primera fila
        // Celda esquina superior izquierda
        tablaIngreso.appendChild(crearHeader("Origen / Destino"));
        
        for (let c = 1; c <= columnas; c++) {
            tablaIngreso.appendChild(crearHeader(c));
        }

        tablaIngreso.appendChild(crearHeader("Oferta"));

        
        // Crear las filas internas

        for (let f = 0; f < filas; f++) {
        
            // A. Colocar el texto de la celda de la 
            const letraFila = String.fromCharCode(65 + f); 
            tablaIngreso.appendChild(crearHeader(letraFila));

            // B. Celdas intermedias: Los inputs correspondientes a esta fila
            for (let c = 0; c < columnas; c++) {
                const clonCelda = tplCelda.content.cloneNode(true);
                const input = clonCelda.querySelector('.input-celda');
                
                input.dataset.fila = letraFila;
                input.dataset.columna = c + 1;
                
                tablaIngreso.appendChild(clonCelda);
            }

            // C. Última celda de la fila: El input o espacio de la "Oferta"
            // Puedes usar el de celda con input para que digiten la oferta
            const clonOferta = tplCelda.content.cloneNode(true);
            tablaIngreso.appendChild(clonOferta);
        }



        // Crear la fila final
        tablaIngreso.appendChild(crearHeader("Demanda"));

        // Inputs para los totales de la demanda (uno por cada columna numérica)
        for (let c = 0; c < columnas; c++) {
            const clonDemanda = tplCelda.content.cloneNode(true);
            tablaIngreso.appendChild(clonDemanda);
        }

        // tablaIngreso.appendChild(crearHeader(""));
    });


    const btnSolPaso = document.getElementById("SolucionPaso");
    const tablaPasos = document.getElementById("tabla_pasos");


    btnSolPaso.addEventListener('click', function(e){ 

        
        spnError.textContent = "";
        guardarDatosIniciales();
        //////////////////////////////////
        //tabla procedimiento
         
        if (guardarDatosIniciales()) {
            CambiarVista("procedimiento");
        }
        else {
            spnError.textContent = "Ingresa un valor a cada celda.";
            return;
        }
        
        const datosGuardados = JSON.parse(localStorage.getItem('datosMatriz')) || {};
        

        let indexCelda = 0;

        //template de encabezado y celda
        const encabezado = document.getElementById("template_header_inicial");
        const celda = document.getElementById("template_celda_input");
         
         
        const filas = Number(slcFilas.value);
        const columnas = Number(slcColumnas.value);
        
        tablaPasos.innerHTML = '';
         
         
        const columnasTotalesGrid = columnas + 2; 
        tablaPasos.style.gridTemplateColumns = `repeat(${columnasTotalesGrid}, 1fr)`;
         
         
        const tplHeader = document.getElementById("template_header_paso");
        const tplCelda = document.getElementById("template_celda_span");
         
        function crearHeader(texto) {
            const clon = tplHeader.content.cloneNode(true);
            clon.querySelector('.texto-header').textContent = texto;
            return clon;
        }
            
        tablaPasos.appendChild(crearHeader("Origen / Destino"));
            
        for (let c = 1; c <= columnas; c++) {
            tablaPasos.appendChild(crearHeader(c));
        }
        
        tablaPasos.appendChild(crearHeader("Oferta"));
            
            
        // Crear las filas internas
        
        for (let f = 0; f < filas; f++) {
            
            const letraFila = String.fromCharCode(65 + f); 
            tablaPasos.appendChild(crearHeader(letraFila));
            
            for (let c = 0; c < columnas; c++) {
                const clonCelda = tplCelda.content.cloneNode(true);
                const span = clonCelda.querySelector('.span-celda');
                
                span.dataset.fila = letraFila;
                span.dataset.columna = c + 1;

                span.textContent = datosGuardados[`celda_${(indexCelda)}`] || '';
                indexCelda++;

                tablaPasos.appendChild(clonCelda);
            }
            
            const clonOferta = tplCelda.content.cloneNode(true);
            const spanOferta = clonOferta.querySelector('.span-celda');
            
            spanOferta.textContent = datosGuardados[`celda_${indexCelda}`] || '';
            indexCelda++;

            tablaPasos.appendChild(clonOferta);
        }
            
            
        tablaPasos.appendChild(crearHeader("Demanda"));
        
        for (let c = 0; c < columnas; c++) {
           const clonDemanda = tplCelda.content.cloneNode(true);
            const spanDemanda = clonDemanda.querySelector('.span-celda');
            
            spanDemanda.textContent = datosGuardados[`celda_${indexCelda}`] || '';
            indexCelda++;
            
            tablaPasos.appendChild(clonDemanda);
        }
            
    });
            
            
    function guardarDatosIniciales() {
        const datos = {};
        const inputs = Array.from(document.querySelectorAll('#tabla_ingreso .input-celda'));
        
        if (inputs.some(input => input.value.trim() === "")) return false;

        inputs.forEach((input, index) => {
            datos[`celda_${index}`] = input.value.trim();
        });
        
        localStorage.setItem('datosMatriz', JSON.stringify(datos));
        return true;
    }
            
            
            
            
            
            
});