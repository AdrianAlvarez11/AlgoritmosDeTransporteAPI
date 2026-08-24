document.addEventListener('DOMContentLoaded', () => {
    const tabla = document.getElementById('tabla_ingreso');
    const error = document.getElementById('error');
    const botonDirecta = document.getElementById('SolucionDirecta');
    const salida = document.getElementById('solucion_directa');
    const formatearNumero = v => Number(v).toLocaleString('es-MX', { maximumFractionDigits: 2 });

    document.getElementById('label_opciones').addEventListener('click', () => document.querySelector('nav').classList.toggle('invisible'));
    document.querySelectorAll('[data-section]').forEach(b => b.addEventListener('click', () => mostrarVista(b.dataset.section)));
    document.getElementById('crearTabla').addEventListener('click', crearTabla);
    botonDirecta.addEventListener('click', () => resolverProblema(false));
    document.getElementById('SolucionPaso').addEventListener('click', () => resolverProblema(true));

    document.getElementById('nav_imprimir').addEventListener('click', () => generarPdf('imprimir'));
    document.getElementById('nav_guardar').addEventListener('click', () => generarPdf('guardar'));

    // Asignar los eventos a los botones si existen, usando validación opcional
    const btnImprimir = document.getElementById('btn_imprimir');
    if (btnImprimir) btnImprimir.addEventListener('click', () => generarPdf('imprimir'));

    const btnGuardar = document.getElementById('btn_guardar');
    if (btnGuardar) btnGuardar.addEventListener('click', () => generarPdf('guardar'));

    async function generarPdf(accion) {
        let datos;
        try { datos = construirProblema(true); } catch (e) { alert(e.message); return; }
        try {
            const r = await fetch('/api/problemas/reporte', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(datos) });
            if (!r.ok) {
                const json = await r.json().catch(() => ({}));
                throw Error((json.errores || ['No se pudo generar el reporte.']).join(' '));
            }
            const blob = await r.blob();
            const url = URL.createObjectURL(blob);
            if (accion === 'guardar') {
                const a = document.createElement('a');
                a.href = url;
                a.download = 'solucion-transporte.pdf';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            } else {
                window.open(url, '_blank');
            }
            setTimeout(() => URL.revokeObjectURL(url), 10000);
        } catch (e) {
            alert(e.message || 'No fue posible conectar con la API.');
        }
    }

    function mostrarVista(id) {
        document.querySelectorAll('main section').forEach(s => s.hidden = s.id !== id);
        sessionStorage.setItem('idVista', id);
    }
    function crearEncabezado(texto) {
        const div = document.createElement('div'); div.className = 'header_inicial'; div.innerHTML = `<span>${texto}</span>`; tabla.append(div);
    }
    function crearEntrada(tipo, fila, columna, etiqueta) {
        const div = document.createElement('div'); div.className = 'celda_inicial';
        const input = document.createElement('input');
        input.type = 'number'; input.min = '0'; input.step = 'any'; input.required = true; input.className = 'input-celda';
        input.dataset.tipo = tipo; input.dataset.fila = fila; input.dataset.columna = columna; input.setAttribute('aria-label', etiqueta);
        div.append(input); tabla.append(div);
    }
    function crearTabla() {
        error.textContent = '';
        const filas = +document.getElementById('filas').value, columnas = +document.getElementById('columnas').value;
        tabla.replaceChildren(); tabla.style.gridTemplateColumns = `repeat(${columnas + 2}, minmax(100px, 1fr))`;
        crearEncabezado('Origen /<br>Destino'); for (let j = 0; j < columnas; j++) crearEncabezado(j + 1); crearEncabezado('Oferta');
        for (let i = 0; i < filas; i++) {
            crearEncabezado(i + 1);
            for (let j = 0; j < columnas; j++) crearEntrada('costo', i, j, `Costo ${i + 1}-${j + 1}`);
            crearEntrada('oferta', i, '', `Oferta ${i + 1}`);
        }
        crearEncabezado('Demanda'); for (let j = 0; j < columnas; j++) crearEntrada('demanda', '', j, `Demanda ${j + 1}`);
    }
    function construirProblema(incluirPasos) {
        const filas = +document.getElementById('filas').value, columnas = +document.getElementById('columnas').value;
        const entradas = [...tabla.querySelectorAll('.input-celda')];
        if (!entradas.length) throw Error('Genera una tabla primero.');
        const valores = new Map();
        for (const e of entradas) {
            const v = +e.value;
            if (e.value.trim() === '' || !Number.isFinite(v) || v < 0) throw Error('Ingresa valores numéricos mayores o iguales a cero en todas las celdas.');
            valores.set(`${e.dataset.tipo}/${e.dataset.fila}/${e.dataset.columna}`, v);
        }
        return {
            metodo: document.getElementById('seleccion_metodo').value, incluirPasos: incluirPasos,
            costos: Array.from({ length: filas }, (_, i) => Array.from({ length: columnas }, (_, j) => valores.get(`costo/${i}/${j}`))),
            ofertas: Array.from({ length: filas }, (_, i) => valores.get(`oferta/${i}/`)),
            demandas: Array.from({ length: columnas }, (_, j) => valores.get(`demanda//${j}`))
        };
    }
    async function resolverProblema(paso) {
        error.textContent = ''; let datos;
        try { datos = construirProblema(paso); } catch (e) { error.textContent = e.message; return; }
        const boton = paso ? document.getElementById('SolucionPaso') : botonDirecta;
        const textoOriginal = boton.textContent;
        boton.disabled = true; boton.textContent = 'Resolviendo...';
        try {
            const r = await fetch('/api/problemas/resolver', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(datos) });
            const json = await r.json();
            if (!r.ok) throw Error((json.errores || ['No se pudo resolver el problema.']).join(' '));
            if (paso) { mostrarPasos(json); mostrarVista('procedimiento'); }
            else { mostrarResultado(json); mostrarVista('resultados'); }
        } catch (e) { error.textContent = e.message || 'No fue posible conectar con la API.'; }
        finally { boton.disabled = false; boton.textContent = textoOriginal; }
    }
    function crearCelda(tablaResultado, contenido, clase) { const d = document.createElement('div'); d.className = clase; d.innerHTML = contenido; tablaResultado.append(d); }
    function mostrarResultado(p) {
        salida.replaceChildren();
        const nombres = { EsquinaNoroeste: 'Esquina Noroeste', CostoMinimo: 'Costo Mínimo', Vogel: 'Vogel' };
        const resumen = document.createElement('div'); resumen.className = 'resumen-solucion';
        resumen.innerHTML = `<strong>${nombres[p.metodo] || p.metodo}</strong><br>Costo total: <strong>${formatearNumero(p.costoTotal)}</strong><br>${p.estaBalanceado ? 'Problema balanceado.' : `${p.balanceAgregado}.`}`;
        salida.append(resumen);
        const t = document.createElement('div'); t.className = 'tabla-resultado'; t.style.gridTemplateColumns = `repeat(${p.matriz[0].length + 2}, minmax(110px, 1fr))`;
        crearCelda(t, 'Origen /<br>Destino', 'header_solucion'); p.matriz[0].forEach((_, j) => crearCelda(t, j + 1, 'header_solucion')); crearCelda(t, 'Oferta', 'header_solucion');
        p.matriz.forEach((fila, i) => { crearCelda(t, i + 1, 'header_solucion'); fila.forEach(c => crearCelda(t, `<span class="costo">Costo: ${formatearNumero(c.costo)}</span><span class="asignacion">${c.asignacion > 0 ? formatearNumero(c.asignacion) : '—'}</span>`, `celda-resultado${c.esFicticia ? ' ficticia' : ''}`)); crearCelda(t, formatearNumero(p.ofertas[i] ?? 0), 'celda-resultado'); });
        crearCelda(t, 'Demanda', 'header_solucion'); p.matriz[0].forEach((_, j) => crearCelda(t, formatearNumero(p.demandas[j] ?? 0), 'celda-resultado'));
        salida.append(t);
    }

    let problemaActual = null;
    let pasoActualIdx = 0;

    document.getElementById('btn_paso_prev').addEventListener('click', () => { if (pasoActualIdx > 0) { pasoActualIdx--; renderizarPaso(); } });
    document.getElementById('btn_paso_next').addEventListener('click', () => { if (pasoActualIdx < problemaActual.pasos.length - 1) { pasoActualIdx++; renderizarPaso(); } });
    document.getElementById('btn_paso_final').addEventListener('click', () => { mostrarResultado(problemaActual); mostrarVista('resultados'); });

    function mostrarPasos(p) {
        problemaActual = p;
        pasoActualIdx = 0;
        renderizarPaso();
    }

    function renderizarPaso() {
        if (!problemaActual || !problemaActual.pasos) return;
        const paso = problemaActual.pasos[pasoActualIdx];
        const nombres = { EsquinaNoroeste: 'Esquina Noroeste', CostoMinimo: 'Costo Mínimo', Vogel: 'Vogel' };

        document.getElementById('encabezado_pasos').innerHTML = `<h2>Solución mediante método de ${nombres[problemaActual.metodo] || problemaActual.metodo}</h2><p><strong>Paso ${paso.numero}:</strong> ${paso.titulo}</p>`;
        document.getElementById('texto_paso').value = paso.descripcion;

        document.getElementById('btn_paso_prev').disabled = pasoActualIdx === 0;
        const esUltimo = pasoActualIdx === problemaActual.pasos.length - 1;
        document.getElementById('btn_paso_next').disabled = esUltimo;
        document.getElementById('btn_paso_final').hidden = !esUltimo;

        const tablaPasos = document.getElementById('tabla_pasos');
        tablaPasos.replaceChildren();

        const matriz = paso.matriz;
        if (!matriz || !matriz.length) return;

        const tienePen = paso.penalizacionesFilas || paso.penalizacionesColumnas;
        const cols = matriz[0].length + 2 + (tienePen ? 1 : 0);
        tablaPasos.style.gridTemplateColumns = `repeat(${cols}, minmax(110px, 1fr))`;

        crearCelda(tablaPasos, 'Origen /<br>Destino', 'header_solucion');
        matriz[0].forEach((_, j) => crearCelda(tablaPasos, j + 1, 'header_solucion'));
        crearCelda(tablaPasos, 'Oferta', 'header_solucion');
        if (tienePen) crearCelda(tablaPasos, 'Penalización', 'header_solucion');

        matriz.forEach((fila, i) => {
            crearCelda(tablaPasos, i + 1, 'header_solucion');
            fila.forEach((c, j) => {
                let extra = '';
                if (c.esFicticia) extra += ' ficticia';
                if (paso.filaSeleccionada === i && paso.columnaSeleccionada === j) extra += ' celda_movimiento';
                crearCelda(tablaPasos, `<span class="costo">Costo: ${formatearNumero(c.costo)}</span><span class="asignacion">${c.asignacion > 0 ? formatearNumero(c.asignacion) : '—'}</span>`, `celda-resultado${extra}`);
            });
            crearCelda(tablaPasos, formatearNumero(paso.ofertasRestantes[i] ?? 0), 'celda-resultado');
            if (tienePen) {
                const pf = paso.penalizacionesFilas && i < paso.penalizacionesFilas.length ? formatearNumero(paso.penalizacionesFilas[i]) : '—';
                crearCelda(tablaPasos, pf, 'celda-resultado');
            }
        });

        crearCelda(tablaPasos, 'Demanda', 'header_solucion');
        matriz[0].forEach((_, j) => crearCelda(tablaPasos, formatearNumero(paso.demandasRestantes[j] ?? 0), 'celda-resultado'));
        crearCelda(tablaPasos, '', 'celda-resultado');
        if (tienePen) {
            crearCelda(tablaPasos, '', 'celda-resultado');
            crearCelda(tablaPasos, 'Penalización', 'header_solucion');
            matriz[0].forEach((_, j) => {
                const pc = paso.penalizacionesColumnas && j < paso.penalizacionesColumnas.length ? formatearNumero(paso.penalizacionesColumnas[j]) : '—';
                crearCelda(tablaPasos, pc, 'celda-resultado');
            });
            crearCelda(tablaPasos, '', 'celda-resultado');
            crearCelda(tablaPasos, '', 'celda-resultado');
        }
    }
});