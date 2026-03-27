console.log(" growth.js LOADED");

window.growthChart = {
    charts: {},

    render: (canvasId, payload) => {
        const el = document.getElementById(canvasId);
        if (!el || !window.Chart || !payload) return;

        const ctx = el.getContext('2d');
        const visitAge = Number(payload.visitAgeMonths ?? 0);
        const infantMode = visitAge <= 36;
        const safeVisitAge = Math.max(visitAge, 1);

  
        let xMin, xMax, step, title;

        if (infantMode) {
            
            xMin = 0;
            xMax = 36;
            step = 3;
            title = "Yaş (ay)";
        } else {
            xMin = 0;
            xMax = Math.ceil(safeVisitAge / 12) * 12;
            step = 12;
            title = "Yaş (yıl)";
        }

        //  AY
        
        const normRefs = (payload.refs || [])
            .map(r => {
                const age =
                    r.ageMonths ?? r.AgeMonths ?? r.yasAy ?? r.YasAy;
                return { ...r, ageMonths: Number(age) };
            })
            .filter(r => !Number.isNaN(r.ageMonths));

        
        const clippedRefs = normRefs.filter(r =>
            r.ageMonths >= xMin && r.ageMonths <= xMax
        );

       
        const patientPoints = (payload.points || [])
            .filter(p => p?.ageMonths != null && p?.value != null)
            .map(p => ({
                x: Number(p.ageMonths),
                y: Number(String(p.value).replace(',', '.'))
            }))
            .filter(p => !Number.isNaN(p.y));


        const zTags = ['-1.881', '-1.282', '-0.674', '0', '+0.674', '+1.282', '+1.881'];
        const labels = ['P3', 'P10', 'P25', 'P50', 'P75', 'P90', 'P97'];

        const refDatasets = zTags.map((tag, i) => {
            const zKey = 'z' + tag;

            const data = clippedRefs
                .map(r => ({
                    x: r.ageMonths,
                    y: Number(r[zKey])
                }))
                .filter(p => !Number.isNaN(p.y));

            return {
                label: labels[i],
                data,
                borderWidth: tag === '0' ? 2 : 1,
                borderDash: tag === '0' ? [] : [4, 4],
                pointRadius: 2,
                fill: false
            };
        });

        const patientDataset = {
            label: (payload.measure || 'Ölçüm') + ' (Hasta)',
            data: patientPoints,
            showLine: true,
            pointRadius: 4,
            borderWidth: 2
        };

      
        if (window.growthChart.charts[canvasId]) {
            window.growthChart.charts[canvasId].destroy();
        }

        window.growthChart.charts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                datasets: [...refDatasets, patientDataset]
            },
            options: {
                responsive: true,
                parsing: false,
                animation: false,
                scales: {
                    x: {
                        type: 'linear',
                        min: xMin,
                        max: xMax,
                        ticks: {
                            stepSize: step,
                            callback: v =>
                                infantMode ? v : (v / 12).toFixed(0)
                        },
                        title: {
                            display: true,
                            text: title
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: payload.measure || ''
                        }
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            title: items => {
                                const x = items?.[0]?.parsed?.x;
                                if (x == null) return '';
                                return infantMode
                                    ? `Yaş: ${x} ay`
                                    : `Yaş: ${(x / 12).toFixed(1)} yıl`;
                            }
                        }
                    }
                },
                elements: {
                    line: { tension: 0.25 }
                }
            }
        });
    }
};
