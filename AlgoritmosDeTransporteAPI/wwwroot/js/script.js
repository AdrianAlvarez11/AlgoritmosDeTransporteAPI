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
        const encabezado = document.getElementById("template_header_inicial");
        const celda = document.getElementById("template_celda_input");


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

    btnSolPaso.addEventListener('click', function(e){ 

        //////////////////////////////////
        //tabla procedimiento
         
        spnError.textContent = "";
        //template de encabezado y celda
        const encabezado = document.getElementById("template_header_inicial");
        const celda = document.getElementById("template_celda_input");
         
         
        const filas = Number(slcFilas.value);
        const columnas = Number(slcColumnas.value);
        
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
            
    });
            
            
            
            
            
            
            
            
            
});