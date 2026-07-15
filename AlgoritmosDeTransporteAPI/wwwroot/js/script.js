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
            clon.querySelector('.texto-header').innerHTML = texto;
            return clon.querySelector('.header_inicial'); 
        }

        //Crear toda la primera fila
        // Celda esquina superior izquierda
        const celdaEsquina = tablaIngreso.appendChild(crearHeader("Origen&nbsp;/ <br> Destino"));
        celdaEsquina.classList.add('columna_fija');
        
        for (let c = 1; c <= columnas; c++) {
            const letraColumna = String.fromCharCode(64 + c);
            tablaIngreso.appendChild(crearHeader(letraColumna));
        }

        tablaIngreso.appendChild(crearHeader("Oferta"));

        
        // Crear las filas internas

        for (let f = 0; f < filas; f++) {
        
            // A. Colocar el texto de la celda de la 
            const numFila = f + 1; 
            const celdaLetra = crearHeader(numFila);
            celdaLetra.classList.add('columna_fija');
            tablaIngreso.appendChild(celdaLetra);

            // B. Celdas intermedias: Los inputs correspondientes a esta fila
            for (let c = 0; c < columnas; c++) {
                const clonCelda = tplCelda.content.cloneNode(true);
                const input = clonCelda.querySelector('.input-celda');
                
                input.dataset.fila = numFila;
                input.dataset.columna = String.fromCharCode(64 + c);
                
                tablaIngreso.appendChild(clonCelda);
            }

            // C. Última celda de la fila: El input o espacio de la "Oferta"
            // Puedes usar el de celda con input para que digiten la oferta
            const clonOferta = tplCelda.content.cloneNode(true);
            tablaIngreso.appendChild(clonOferta);
        }



        // Crear la fila final
        const celdaDemanda = tablaIngreso.appendChild(crearHeader("Demanda")).classList.add("columna_fija");

        // Inputs para los totales de la demanda (uno por cada columna numérica)
        for (let c = 0; c < columnas; c++) {
            const clonDemanda = tplCelda.content.cloneNode(true);
            tablaIngreso.appendChild(clonDemanda);
        }

        // tablaIngreso.appendChild(crearHeader(""));
    });


    const btnSolPaso = document.getElementById("SolucionPaso");
    const btnSolDirecta = document.getElementById("SolucionDirecta");
    const tablaPasos = document.getElementById("tabla_pasos");


    btnSolPaso.addEventListener('click', function(e){ 

        
        spnError.textContent = "";
        //guardarDatosIniciales();
        //////////////////////////////////
        //tabla procedimiento
         
        let mensajeError = guardarDatosIniciales();
        if (mensajeError == "") {
            CambiarVista("procedimiento");
        }
        else {
            spnError.textContent = mensajeError;
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
            clon.querySelector('.texto-header').innerHTML = texto;
            return clon.querySelector('.header_solucion'); 
        }
            
        const celdaEsquina = tablaPasos.appendChild(crearHeader("Origen&nbsp;/ <br> Destino"));
        celdaEsquina.classList.add('columna_fija');
        
        for (let c = 1; c <= columnas; c++) {
            const letraColumna = String.fromCharCode(64 + c);
            tablaPasos.appendChild(crearHeader(letraColumna));
        }
        
        tablaPasos.appendChild(crearHeader("Oferta"));
            
            
        // Crear las filas internas
        
        for (let f = 0; f < filas; f++) {
            const numFila = f + 1; 
            const celdaLetra = tablaPasos.appendChild(crearHeader(numFila));
            celdaLetra.classList.add("columna_fija");
            
            for (let c = 0; c < columnas; c++) {
                const clonCelda = tplCelda.content.cloneNode(true);
                const span = clonCelda.querySelector('.span-celda');
                
                span.dataset.fila = numFila;
                span.dataset.columna = String.fromCharCode(64 + c);

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
        
            
        const celdaDemanda = tablaPasos.appendChild(crearHeader("Demanda")).classList.add("columna_fija");
        
        for (let c = 0; c < columnas; c++) {
           const clonDemanda = tplCelda.content.cloneNode(true);
            const spanDemanda = clonDemanda.querySelector('.span-celda');
            
            spanDemanda.textContent = datosGuardados[`celda_${indexCelda}`] || '';
            indexCelda++;
            
            tablaPasos.appendChild(clonDemanda);
        }
            
    });

    btnSolDirecta.addEventListener('click', function(e){ 
        spnError.textContent = "";
         
        let mensajeError = guardarDatosIniciales();
        if (mensajeError == "") {
            CambiarVista("resultados");
        }
        else {
            spnError.textContent = mensajeError;
            return;
        }
    });
            
            
    function guardarDatosIniciales() {
        const datos = {};
        const inputs = Array.from(document.querySelectorAll('#tabla_ingreso .input-celda'));
        
        if(inputs.length == 0) return "Genera una tabla primero.";
        if (inputs.some(input => input.value.trim() === "")) return "Ingresa un valor a cada celda.";

        inputs.forEach((input, index) => {
            datos[`celda_${index}`] = input.value.trim();
        });
        
        localStorage.setItem('datosMatriz', JSON.stringify(datos));
        return "";
    }
            
            
            
            
            
    tablaIngreso.addEventListener('focusin', function(e) {
        if (e.target.classList.contains('input-celda')) {
            const contenedor = tablaIngreso;
            const celdaActual = e.target.closest('.celda_inicial');
            if (!celdaActual) return;

            const anchoColumnaFija = 104; 

            //Posición física de la celda dentro de la matriz Grid
            const celdaLeft = celdaActual.offsetLeft;
            const celdaWidth = celdaActual.offsetWidth;

            //Límites visibles del contenedor con scroll
            const scrollLeftActual = contenedor.scrollLeft;
            const anchoVisibleContenedor = contenedor.clientWidth;

            //Moviéndose a la derecha -> Si la celda se sale por el borde derecho de la pantalla
            if ((celdaLeft + celdaWidth) > (scrollLeftActual + anchoVisibleContenedor)) {
                contenedor.scrollTo({
                    left: celdaLeft + celdaWidth - anchoVisibleContenedor + 12,
                    behavior: 'smooth'
                });
            }
            
            //Moviéndose a la izquierda -> Forzar a que la celda se alinee al lado de las letras
            else if (celdaLeft < (scrollLeftActual + anchoColumnaFija)) {
                contenedor.scrollTo({
                    left: celdaLeft - anchoColumnaFija - 14,
                    behavior: 'smooth'
                });
            }
        }
    });
            
});
