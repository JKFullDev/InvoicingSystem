window.renderCustomerChart = (canvasId, fechas, totales) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destruye el gráfico anterior si ya existía en ese hueco
    let chartStatus = Chart.getChart(canvasId);
    if (chartStatus != undefined) {
        chartStatus.destroy();
    }

    new Chart(canvas, {
        type: 'bar', // Puede ser 'line', 'pie', etc.
        data: {
            labels: fechas,
            datasets: [{
                label: 'Facturación (€)',
                data: totales,
                backgroundColor: 'rgba(54, 162, 235, 0.5)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
};