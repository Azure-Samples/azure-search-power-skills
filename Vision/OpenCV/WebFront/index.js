'use strict';

$(() => {
    const maxImgSize = 600;
    const spinner = $("#spinner");
    const imageSetMenu = $("#image-set-menu");
    const carousel = $("#carousel-custom");
    const imgStrip = $("#img-strip");
    const diffRightResolution = $("#diff-right-resolution");
    const diffRightImg = $("#diff-right-img");
    const swipeRightImg = $("#swipe-right-img");
    const onionRightImg = $("#onion-right-img");
    const diffLeft = $("#diff-left");
    const diffLeftResolution = $("#diff-left-resolution");
    const diffLeftImg = $("#diff-left-img");
    const swipeLeftImg = $("#swipe-left-img");
    const onionLeftImg = $("#onion-left-img");
    const sideBySideView = $("#diff-side-by-side-view");
    const swipeView = $("#diff-swipe-view");
    const onionView = $("#diff-onion-view");
    const diffView = $("#diff-diff-view");
    const rightImages = [diffRightImg, swipeRightImg, onionRightImg];
    const leftImages = [diffLeftImg, swipeLeftImg, onionLeftImg];

    let imageSet = [];
    let currentIndex = 0;

    function imgSrc(imgData) {
        return imgData.data ? `data:${imgData.mimetype};base64,${imgData.data}` : imgData.dataUrl;
    }

    function initImageStrip(images, callback) {
        let remaining = images.length;
        imgStrip.empty();
        images.forEach((imgData, i) => {
            const li = $(`<li data-index="${i}"></li>`);
            if (i === 0) {
                li.addClass("active");
            }
            const img = $('<img class="align-middle">');
            const src = imgSrc(imgData);
            img.attr("alt", imgData.name);
            if (src.substr(0, 5) !== "data:") {
                img.prop("crossOrigin", "anonymous");
            }
            img.prop("loading", "eager");
            console.log(`Queueing image #${i}.`)
            img.get(0).addEventListener("load", e => {
                console.log(`Loaded image ${ e.target.alt } ... ${remaining} remaining.`)
                if (--remaining === 0) {
                    callback();
                }
            });
            img.attr("src", src);
            li.append(img);
            imgStrip.append(li);
        });
    }

    function getScale(width, height, max) {
        return max / (width > height ? width : height);
    }

    function setScale(img, scale, naturalWidth, naturalHeight) {
        img.css("width", scale * naturalWidth + "px");
        img.css("height", scale * naturalHeight + "px");
    }

    function getPixelDiff(leftImg, rightImg, width, height) {
        width = Math.round(width);
        height = Math.round(height);
        const leftCanvas = document.createElement("canvas");
        const rightCanvas = document.createElement("canvas");
        const diffCanvas = document.createElement("canvas");
        diffCanvas.width = rightCanvas.width = leftCanvas.width = Math.round(width);
        diffCanvas.height = rightCanvas.height = leftCanvas.height = Math.round(height);
        const leftContext = leftCanvas.getContext('2d');
        const rightContext = rightCanvas.getContext('2d');
        const diffContext = diffCanvas.getContext('2d');
        leftContext.drawImage(leftImg, 0, 0, width, height);
        rightContext.drawImage(rightImg, 0, 0, width, height);
        const diff = diffContext.createImageData(width, height);
        try {
            pixelmatch(
                leftContext.getImageData(0, 0, width, height).data,
                rightContext.getImageData(0, 0, width, height).data,
                diff.data,
                width,
                height,
                { threshold: 0.1 });
            diffContext.putImageData(diff, 0, 0);
        }
        finally {
            return diffCanvas;
        }
    }

    function selectImage(index) {
        currentIndex = index;
        imgStrip.find("li").removeClass("active");
        const selectedImg = imgStrip.find(`li[data-index='${index}'] img`);
        selectedImg.parent().addClass("active");
        const rightSrc = selectedImg.attr("src");
        rightImages.forEach(img => img.attr("src", rightSrc));
        const rightWidth = selectedImg.prop('naturalWidth');
        const rightHeight = selectedImg.prop('naturalHeight');
        console.log(`Right image ${rightWidth} x ${rightHeight}`)
        diffRightResolution.html(`${rightWidth} &times; ${rightHeight}`);
        const rightScale = getScale(rightWidth, rightHeight, maxImgSize);
        setScale(swipeRightImg, rightScale, rightWidth, rightHeight);
        setScale(onionRightImg, rightScale, rightWidth, rightHeight);

        if (index <= 0) {
            leftImages.forEach(img => img.attr("src", ""));
            diffLeftResolution.empty();
            diffLeft.addClass("hidden");
            setScale(diffRightImg, rightScale, rightWidth, rightHeight);
            diffView.empty();
        }
        else {
            diffLeft.removeClass("hidden");
            const previousImg = imgStrip.find(`li[data-index='${index - 1}'] img`);
            const leftSrc = previousImg.attr("src");
            leftImages.forEach(img => img.attr("src", leftSrc));
            const leftWidth = previousImg.prop('naturalWidth');
            const leftHeight = previousImg.prop('naturalHeight');
            console.log(`Left image ${leftWidth} x ${leftHeight}`)
            diffLeftResolution.html(`${leftWidth} &times; ${leftHeight}`);
            const leftScale = getScale(leftWidth, leftHeight, maxImgSize);
            setScale(swipeLeftImg, leftScale, leftWidth, leftHeight);
            setScale(onionLeftImg, leftScale, leftWidth, leftHeight);
            const scale = Math.min(leftScale, rightScale);
            setScale(diffRightImg, scale, rightWidth, rightHeight);
            setScale(diffLeftImg, scale, leftWidth, leftHeight);
            const diffCanvas = getPixelDiff(previousImg.get(0), selectedImg.get(0), leftWidth * leftScale, leftHeight * leftScale);
            diffView.empty();
            diffView.append(diffCanvas);
        }
    }

    function loadImageSet(sample) {
        imageSet = sample.data;

        carousel.addClass("hidden");
        spinner.removeClass("hidden");

        initImageStrip(imageSet, () => {
            spinner.addClass("hidden");

            selectImage(imageSet.length - 1);

            carousel.removeClass("hidden");
        });
    }

    $("#img-strip").on("click", "li", e => {
        const selectedIndex = parseInt($(e.currentTarget).data("index"), 10);
        selectImage(selectedIndex);
    });

    $(".carousel-control.left").click(e => {
        if (currentIndex > 0) {
            selectImage(currentIndex - 1);
        }
    });

    $(".carousel-control.right").click(e => {
        if (currentIndex < imageSet.length - 1) {
            selectImage(currentIndex + 1);
        }
    });

    $("input[type=radio][name=diff-view]").change(e => {
        const selection = $(e.delegateTarget).data("selects");
        [sideBySideView, swipeView, onionView, diffView]
            .forEach(view => {
                if (view.attr("id") == selection) {
                    view.removeClass("hidden");
                }
                else {
                    view.addClass("hidden");
                }
            });
    });

    $("#diff-swipe-slider").change(e => {
        const percentage = parseFloat($(e.delegateTarget).val());
        const width = swipeRightImg.prop("offsetWidth") + 1;
        swipeRightImg.css("clip", `rect(0, ${width * percentage / 100}px, auto, auto)`);
    });

    $("#diff-onion-slider").change(e => {
        const percentage = parseFloat($(e.delegateTarget).val());
        onionRightImg.css("opacity", (percentage / 100));
    });

    imageSetMenu.on("click", "button", e => {
        const sampleName = $(e.currentTarget).data("sample");
        const sample = dataset.find(sample => sample.name === sampleName);
        if (sample) loadImageSet(sample);
    });

    dataset.forEach(sample => {
        imageSetMenu.append($(`<button class="dropdown-item" data-sample="${sample.name}">${sample.name}</a>`));
    });

});