const express = require('express');
const app = express();

app.use(express.json());

app.get('/api/getKelvinTemperature/:temp', (req, res) => {
    const { temp } = req.params;
    console.log('temp :>> ', temp);
    res.send(Number(temp) + 273.15);
});

app.post('/api/drinks/make-lemonade', (req, res) => {
    console.log('received lemonade order!');

    if (req.body.sugarSpoons < 1) {
        return res.status(400).send({
            isSuccessful: false,
            message: 'invalid spoon count!'
        });
    }

    return res.send({
        isSuccessful: true,
        secretIngridient: "Mashrooms"
    });
});

app.post('/api/food/make-pizza', (req, res) => {
    console.log('received pizza order!');

    res.send({
        isSuccessful: true,
    });
});

app.listen(3000, () => console.log('started on port 3000!'))