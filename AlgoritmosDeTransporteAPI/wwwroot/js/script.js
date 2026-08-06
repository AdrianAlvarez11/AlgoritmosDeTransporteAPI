document.addEventListener('DOMContentLoaded', () => {
  const tabla = document.getElementById('tabla_ingreso');
  const error = document.getElementById('error');
  const botonDirecta = document.getElementById('SolucionDirecta');
  const salida = document.getElementById('solucion_directa');
  const letra = i => String.fromCharCode(65 + i);
  const numero = v => Number(v).toLocaleString('es-MX', { maximumFractionDigits: 2 });

  document.getElementById('label_opciones').addEventListener('click', () => document.querySelector('nav').classList.toggle('invisible'));
  document.querySelectorAll('[data-section]').forEach(b => b.addEventListener('click', () => vista(b.dataset.section)));
  document.getElementById('crearTabla').addEventListener('click', crearTabla);
  botonDirecta.addEventListener('click', resolver);
  document.getElementById('SolucionPaso').addEventListener('click', () => error.textContent = 'La solución paso a paso se integrará después de la solución directa.');

  function vista(id) {
    document.querySelectorAll('main section').forEach(s => s.hidden = s.id !== id);
    sessionStorage.setItem('idVista', id);
  }
  function encabezado(texto) {
    const div = document.createElement('div'); div.className = 'header_inicial'; div.innerHTML = `<span>${texto}</span>`; tabla.append(div);
  }
  function entrada(tipo, fila, columna, etiqueta) {
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
    encabezado('Origen /<br>Destino'); for (let j = 0; j < columnas; j++) encabezado(letra(j)); encabezado('Oferta');
    for (let i = 0; i < filas; i++) {
      encabezado(`Origen ${i + 1}`);
      for (let j = 0; j < columnas; j++) entrada('costo', i, j, `Costo ${i + 1}-${letra(j)}`);
      entrada('oferta', i, '', `Oferta ${i + 1}`);
    }
    encabezado('Demanda'); for (let j = 0; j < columnas; j++) entrada('demanda', '', j, `Demanda ${letra(j)}`);
  }
  function problema() {
    const filas = +document.getElementById('filas').value, columnas = +document.getElementById('columnas').value;
    const entradas = [...tabla.querySelectorAll('.input-celda')];
    if (!entradas.length) throw Error('Genera una tabla primero.');
    const valores = new Map();
    for (const e of entradas) {
      const v = +e.value;
      if (e.value.trim() === '' || !Number.isFinite(v) || v < 0) throw Error('Ingresa valores numéricos mayores o iguales a cero en todas las celdas.');
      valores.set(`${e.dataset.tipo}/${e.dataset.fila}/${e.dataset.columna}`, v);
    }
    return { metodo: document.getElementById('seleccion_metodo').value, incluirPasos: false,
      costos: Array.from({length: filas}, (_, i) => Array.from({length: columnas}, (_, j) => valores.get(`costo/${i}/${j}`))),
      ofertas: Array.from({length: filas}, (_, i) => valores.get(`oferta/${i}/`)),
      demandas: Array.from({length: columnas}, (_, j) => valores.get(`demanda//${j}`)) };
  }
  async function resolver() {
    error.textContent = ''; let datos;
    try { datos = problema(); } catch (e) { error.textContent = e.message; return; }
    botonDirecta.disabled = true; botonDirecta.textContent = 'Resolviendo...';
    try {
      const r = await fetch('/api/problemas/resolver', { method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(datos) });
      const json = await r.json();
      if (!r.ok) throw Error((json.errores || ['No se pudo resolver el problema.']).join(' '));
      mostrar(json); vista('resultados');
    } catch (e) { error.textContent = e.message || 'No fue posible conectar con la API.'; }
    finally { botonDirecta.disabled = false; botonDirecta.textContent = 'Solución directa'; }
  }
  function celda(tablaResultado, contenido, clase) { const d = document.createElement('div'); d.className = clase; d.innerHTML = contenido; tablaResultado.append(d); }
  function mostrar(p) {
    salida.replaceChildren();
    const nombres = { EsquinaNoroeste: 'Esquina Noroeste', CostoMinimo: 'Costo Mínimo', Vogel: 'Vogel' };
    const resumen = document.createElement('div'); resumen.className = 'resumen-solucion';
    resumen.innerHTML = `<strong>${nombres[p.metodo] || p.metodo}</strong><br>Costo total: <strong>${numero(p.costoTotal)}</strong><br>${p.estaBalanceado ? 'Problema balanceado.' : `${p.balanceAgregado}.`}`;
    salida.append(resumen);
    const t = document.createElement('div'); t.className = 'tabla-resultado'; t.style.gridTemplateColumns = `repeat(${p.matriz[0].length + 2}, minmax(110px, 1fr))`;
    celda(t, 'Origen /<br>Destino', 'header_solucion'); p.matriz[0].forEach((_, j) => celda(t, letra(j), 'header_solucion')); celda(t, 'Oferta', 'header_solucion');
    p.matriz.forEach((fila, i) => { celda(t, `Origen ${i + 1}`, 'header_solucion'); fila.forEach(c => celda(t, `<span class="costo">Costo: ${numero(c.costo)}</span><span class="asignacion">${c.asignacion > 0 ? numero(c.asignacion) : '—'}</span>`, `celda-resultado${c.esFicticia ? ' ficticia' : ''}`)); celda(t, numero(p.ofertas[i] ?? 0), 'celda-resultado'); });
    celda(t, 'Demanda', 'header_solucion'); p.matriz[0].forEach((_, j) => celda(t, numero(p.demandas[j] ?? 0), 'celda-resultado'));
    salida.append(t);
  }
});
